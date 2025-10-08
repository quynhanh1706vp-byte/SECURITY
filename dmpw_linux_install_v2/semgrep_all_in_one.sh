\
#!/usr/bin/env bash
set -euo pipefail

# semgrep_all_in_one.sh
# Usage:
#   ./semgrep_all_in_one.sh [TARGET_DIR] [JOBS] [--pro]
# Examples:
#   ./semgrep_all_in_one.sh                  # scan current dir, 4 jobs
#   ./semgrep_all_in_one.sh . 6 --pro        # scan current dir, 6 jobs, use Pro rules (if logged in)
#
# Outputs in TARGET_DIR:
#   - semgrep-run.log
#   - semgrep-results.sarif
#   - semgrep-results.top10.csv
#   - semgrep-findings-all.csv

TARGET_DIR="${1:-$PWD}"
JOBS="${2:-4}"
USE_PRO=0
if [[ "${3:-}" == "--pro" ]] || [[ "${1:-}" == "--pro" ]] || [[ "${2:-}" == "--pro" ]]; then
  USE_PRO=1
fi

cd "$TARGET_DIR"

# ---------- sanity checks ----------
if ! command -v docker >/dev/null 2>&1; then
  echo "[ERROR] Docker is required. Please install Docker and retry."
  exit 2
fi

echo "[INFO] Target dir   : $PWD"
echo "[INFO] Jobs         : $JOBS"
echo "[INFO] Pro rules    : $([[ $USE_PRO -eq 1 ]] && echo 'ON' || echo 'OFF')"

# ---------- prep outputs ----------
LOG_FILE="semgrep-run.log"
SARIF_FILE="semgrep-results.sarif"
TOP10_CSV="semgrep-results.top10.csv"
ALL_CSV="semgrep-findings-all.csv"
: > "$LOG_FILE"
rm -f "$SARIF_FILE" "$TOP10_CSV" "$ALL_CSV"

# ---------- optional extraction ----------
EXTRACT_ROOT="semgrep_extracted"
mkdir -p "$EXTRACT_ROOT"
# Extract known archives into EXTRACT_ROOT/<name-without-ext>
shopt -s nullglob
found_archives=0
for gz in code/*.tar.gz code/**/*.tar.gz install/*.tar.gz install/**/*.tar.gz *.tar.gz; do
  base="$(basename "$gz" .tar.gz)"
  dest="$EXTRACT_ROOT/$base"
  mkdir -p "$dest"
  echo "[INFO] Extracting $gz -> $dest" | tee -a "$LOG_FILE"
  tar -xzf "$gz" -C "$dest" || true
  found_archives=$((found_archives+1))
done
shopt -u nullglob
if [[ $found_archives -gt 0 ]]; then
  echo "[INFO] Extracted $found_archives archive(s) into $EXTRACT_ROOT/" | tee -a "$LOG_FILE"
else
  echo "[INFO] No archives (*.tar.gz) found to extract; continuing." | tee -a "$LOG_FILE"
fi

# ---------- build docker args ----------
DOCKER_IMG="returntocorp/semgrep"
DOCKER_RUN=( docker run --rm -u "$(id -u)":"$(id -g)" -v "$PWD":/src -w /src )

# Mount ~/.semgrep if --pro to enable Semgrep Registry Pro rules (after 'semgrep login')
if [[ $USE_PRO -eq 1 ]]; then
  if [[ -d "$HOME/.semgrep" ]]; then
    DOCKER_RUN+=( -v "$HOME/.semgrep":/home/semgrep/.semgrep )
    echo "[INFO] Mounting ~/.semgrep for Pro rules" | tee -a "$LOG_FILE"
  else
    echo "[WARN] --pro passed but $HOME/.semgrep not found. Run:" | tee -a "$LOG_FILE"
    echo "       docker run --rm -it -v \"$HOME/.semgrep\":/home/semgrep/.semgrep $DOCKER_IMG semgrep login" | tee -a "$LOG_FILE"
  fi
fi

# ---------- run semgrep scan ----------
# Rulesets chosen for high coverage; adjust as needed.
SCAN_ARGS=(
  semgrep scan
  --config p/owasp-top-ten
  --config p/r2c-security-audit
  --config p/secrets
  --config p/javascript
  --config p/typescript
  --config p/python
  --config p/csharp
  --config p/dockerfile
  --config p/iac
  --include "$EXTRACT_ROOT/**"
  --include "code/**"
  --include "install/extracted/**"
  --include "."
  --exclude node_modules --exclude dist --exclude build --exclude bin --exclude obj --exclude .git
  --exclude '*.tar*' --exclude '*.zip' --exclude '*.pdf'
  --no-git-ignore
  --timeout 1200
  --max-target-bytes 1000000000
  --jobs "$JOBS"
  --metrics off
  --sarif -o "/src/$SARIF_FILE"
  --error
  --verbose
)

echo "[INFO] Using Docker image: $DOCKER_IMG" | tee -a "$LOG_FILE"
# shellcheck disable=SC2068
"${DOCKER_RUN[@]}" "$DOCKER_IMG" ${SCAN_ARGS[@]} |& tee -a "$LOG_FILE"

# ---------- generate Top 10 CSV + Full findings CSV from SARIF ----------
/usr/bin/env python3 - <<'PY' "$SARIF_FILE" "$TOP10_CSV" "$ALL_CSV"
import sys, json, csv
sarif_path, top10_csv, all_csv = sys.argv[1], sys.argv[2], sys.argv[3]

def sev(level: str) -> str:
    l=(level or "").lower()
    if l in ("error","high"): return "HIGH"
    if l in ("warning","medium"): return "MEDIUM"
    if l in ("note","low"): return "LOW"
    return "INFO"

data = json.load(open(sarif_path, encoding="utf-8"))
runs = data.get("runs", [])
results = []
rules_idx = {}
for run in runs:
    rules_idx = {r.get("id"): r for r in (run.get("tool", {}) \
        .get("driver", {}).get("rules", []) or [])}
    for r in run.get("results", []):
        rid = r.get("ruleId","")
        rr  = rules_idx.get(rid, {})
        level = r.get("level") or rr.get("defaultConfiguration",{},).get("level") \
                or rr.get("properties",{}).get("problem.severity") or ""
        name = rr.get("shortDescription",{}).get("text") or rr.get("name") or rid or "Unknown"
        msg  = (r.get("message",{}) or {}).get("text","")
        locs = r.get("locations",[])
        if not locs:
            results.append({"Severity": sev(level), "Rule": name, "Rule ID": rid, "File":"", "Line":"", "Message": msg})
        else:
            for loc in locs:
                phys = (loc.get("physicalLocation",{}) or {})
                file = (phys.get("artifactLocation",{}) or {}).get("uri","")
                line = (phys.get("region",{}) or {}).get("startLine","")
                results.append({"Severity": sev(level), "Rule": name, "Rule ID": rid, "File":file, "Line":line, "Message": msg})

# Write all findings (ticket-ready)
with open(all_csv,"w",newline="",encoding="utf-8") as f:
    w=csv.DictWriter(f, fieldnames=["Severity","Rule","Rule ID","File","Line","Message"])
    w.writeheader(); w.writerows(results)

# Compute top10 by (severity, count)
from collections import Counter
order={"HIGH":0,"MEDIUM":1,"LOW":2,"INFO":3}
counter = Counter((r["Rule"], r["Severity"]) for r in results)
top = sorted(counter.items(), key=lambda kv:(order.get(kv[0][1],9), -kv[1], kv[0][0]))[:10]

rows=[["Severity","Rule","Count","Sample File","Line","Message"]]
for (rule, sev_), cnt in top:
    samples=[r for r in results if r["Rule"]==rule and r["Severity"]==sev_][:3]
    for i,s in enumerate(samples,1):
        rows.append([sev_, rule, cnt if i==1 else "", s["File"], s["Line"], s["Message"][:200]])

with open(top10_csv,"w",newline="",encoding="utf-8") as f:
    csv.writer(f).writerows(rows)

print(f"[DONE] Wrote: {top10_csv} and {all_csv}")
PY

echo ""
echo "========================================"
echo " Done. Outputs in: $PWD"
echo " - $LOG_FILE"
echo " - $SARIF_FILE"
echo " - $TOP10_CSV"
echo " - $ALL_CSV"
echo "========================================"
