\
#!/usr/bin/env bash
set -euo pipefail

# semgrep_all_in_one_v2.sh — Advanced SAST one-shot runner
# Features:
#  - Auto-extract *.tar.gz into a timestamped report dir
#  - High-coverage Semgrep scan via Docker (UID/GID mapped to avoid permission issues)
#  - Optional Pro rules (mounts ~/.semgrep if --pro)
#  - Flexible rule packs (--full, --auto, --audit, --secrets, --dockerfile, --iac)
#  - Includes/Excludes controls, respect or ignore .gitignore
#  - Tunables: --jobs, --timeout, --max-bytes
#  - Outputs: reports/security/<TS>/{semgrep-run.log, semgrep-results.sarif, semgrep-results.top10.csv, semgrep-findings-all.csv, summary.{json,md}}
#  - Gating: --fail-on HIGH,MEDIUM (exit non-zero if any such severities found)
#
# Usage:
#   ./semgrep_all_in_one_v2.sh [TARGET_DIR=.]
#   ./semgrep_all_in_one_v2.sh . --full --pro --jobs 6 --fail-on HIGH,MEDIUM
#
# Notes:
#   - Requires Docker. If using Pro rules, run: docker run --rm -it -v "$HOME/.semgrep":/home/semgrep/.semgrep returntocorp/semgrep semgrep login

TARGET_DIR="${1:-.}"; shift || true

# ---- defaults ----
JOBS="${JOBS:-}"
TIMEOUT=1200
MAX_BYTES=1000000000
USE_PRO=0
USE_FULL=0
USE_AUTO=0
USE_AUDIT=1         # on by default
USE_SECRETS=1       # on by default
USE_DOCKERFILE=0
USE_IAC=0
RESPECT_GITIGNORE=0
EXTRACT=1
FAIL_ON=""
INCLUDE_PATTERNS=()
EXCLUDE_PATTERNS=( "node_modules" "dist" "build" "bin" "obj" ".git" "*.tar*" "*.zip" "*.pdf" )

# ---- parse args ----
while [[ $# -gt 0 ]]; do
  case "$1" in
    --pro) USE_PRO=1; shift;;
    --full) USE_FULL=1; USE_AUTO=1; USE_DOCKERFILE=1; USE_IAC=1; shift;;
    --auto) USE_AUTO=1; shift;;
    --audit) USE_AUDIT=1; shift;;
    --no-audit) USE_AUDIT=0; shift;;
    --secrets) USE_SECRETS=1; shift;;
    --no-secrets) USE_SECRETS=0; shift;;
    --dockerfile) USE_DOCKERFILE=1; shift;;
    --iac) USE_IAC=1; shift;;
    --respect-gitignore) RESPECT_GITIGNORE=1; shift;;
    --no-extract) EXTRACT=0; shift;;
    --jobs) JOBS="${2:-}"; shift 2;;
    --timeout) TIMEOUT="${2:-1200}"; shift 2;;
    --max-bytes) MAX_BYTES="${2:-1000000000}"; shift 2;;
    --include) INCLUDE_PATTERNS+=( "$2" ); shift 2;;
    --exclude) EXCLUDE_PATTERNS+=( "$2" ); shift 2;;
    --fail-on) FAIL_ON="${2:-}"; shift 2;;
    *) echo "[WARN] Unknown option: $1"; shift;;
  esac
done

if ! command -v docker >/dev/null 2>&1; then
  echo "[ERROR] Docker is required."; exit 2
fi

cd "$TARGET_DIR"
if [[ -z "${JOBS}" ]]; then
  if command -v nproc >/dev/null 2>&1; then JOBS="$(nproc)"; else JOBS=4; fi
fi

TS="$(date +%Y%m%d_%H%M%S)"
REPORT_DIR="reports/security/${TS}"
EXTRACT_DIR="${REPORT_DIR}/extracted"
mkdir -p "${REPORT_DIR}"
LOG_FILE="${REPORT_DIR}/semgrep-run.log"
SARIF_FILE="${REPORT_DIR}/semgrep-results.sarif"
TOP10_CSV="${REPORT_DIR}/semgrep-results.top10.csv"
ALL_CSV="${REPORT_DIR}/semgrep-findings-all.csv"
SUMMARY_JSON="${REPORT_DIR}/summary.json"
SUMMARY_MD="${REPORT_DIR}/summary.md"

echo "[INFO] Target dir        : $PWD" | tee -a "$LOG_FILE"
echo "[INFO] Report dir        : $REPORT_DIR" | tee -a "$LOG_FILE"
echo "[INFO] Jobs              : $JOBS" | tee -a "$LOG_FILE"
echo "[INFO] Timeout / MaxBytes: ${TIMEOUT}s / ${MAX_BYTES}" | tee -a "$LOG_FILE"
echo "[INFO] Pro rules         : $([[ $USE_PRO -eq 1 ]] && echo ON || echo OFF)" | tee -a "$LOG_FILE"
echo "[INFO] Full pack         : $([[ $USE_FULL -eq 1 ]] && echo ON || echo OFF)" | tee -a "$LOG_FILE"
echo "[INFO] Respect .gitignore: $([[ $RESPECT_GITIGNORE -eq 1 ]] && echo ON || echo OFF)" | tee -a "$LOG_FILE"
echo "[INFO] Extract archives  : $([[ $EXTRACT -eq 1 ]] && echo ON || echo OFF)" | tee -a "$LOG_FILE"
echo "[INFO] Fail gate         : ${FAIL_ON:-NONE}" | tee -a "$LOG_FILE"

# ---- extract archives ----
if [[ $EXTRACT -eq 1 ]]; then
  mkdir -p "$EXTRACT_DIR"
  shopt -s nullglob
  count=0
  for gz in code/*.tar.gz code/**/*.tar.gz install/*.tar.gz install/**/*.tar.gz *.tar.gz; do
    base="$(basename "$gz" .tar.gz)"
    dest="${EXTRACT_DIR}/${base}"
    mkdir -p "$dest"
    echo "[INFO] Extracting $gz -> $dest" | tee -a "$LOG_FILE"
    tar -xzf "$gz" -C "$dest" || true
    count=$((count+1))
  done
  shopt -u nullglob
  echo "[INFO] Extracted ${count} archive(s)" | tee -a "$LOG_FILE"
fi

# ---- build docker & semgrep args ----
DOCKER_IMG="returntocorp/semgrep"
DOCKER_RUN=( docker run --rm -u "$(id -u)":"$(id -g)" -v "$PWD":/src -w /src )
if [[ $USE_PRO -eq 1 && -d "$HOME/.semgrep" ]]; then
  DOCKER_RUN+=( -v "$HOME/.semgrep":/home/semgrep/.semgrep )
fi

SCAN_ARGS=( semgrep scan )

# Baseline packs
SCAN_ARGS+=( --config p/owasp-top-ten )
[[ $USE_AUDIT -eq 1 ]]   && SCAN_ARGS+=( --config p/r2c-security-audit )
[[ $USE_SECRETS -eq 1 ]] && SCAN_ARGS+=( --config p/secrets )
# Languages
SCAN_ARGS+=( --config p/javascript --config p/typescript --config p/python --config p/csharp )
[[ $USE_DOCKERFILE -eq 1 ]] && SCAN_ARGS+=( --config p/dockerfile )
[[ $USE_IAC -eq 1 ]]        && SCAN_ARGS+=( --config p/iac )
[[ $USE_AUTO -eq 1 ]]       && SCAN_ARGS+=( --config auto )

# Includes/Excludes
SCAN_ARGS+=( --include "${EXTRACT_DIR}/**" --include "code/**" --include "install/extracted/**" --include "." )
for inc in "${INCLUDE_PATTERNS[@]:-}"; do SCAN_ARGS+=( --include "$inc" ); done
if [[ ${#EXCLUDE_PATTERNS[@]} -gt 0 ]]; then
  for exc in "${EXCLUDE_PATTERNS[@]}"; do SCAN_ARGS+=( --exclude "$exc" ); done
fi
# Gitignore behavior
if [[ $RESPECT_GITIGNORE -eq 1 ]]; then
  : # default behavior respects .gitignore
else
  SCAN_ARGS+=( --no-git-ignore )
fi

SCAN_ARGS+=( --timeout "${TIMEOUT}" --max-target-bytes "${MAX_BYTES}" --jobs "${JOBS}" --metrics off )
SCAN_ARGS+=( --sarif -o "/src/${SARIF_FILE}" --error --verbose )

echo "[INFO] Using Docker image: $DOCKER_IMG" | tee -a "$LOG_FILE"
# shellcheck disable=SC2068
"${DOCKER_RUN[@]}" "$DOCKER_IMG" ${SCAN_ARGS[@]} |& tee -a "$LOG_FILE"

# ---- post-process: build CSVs & summary ----
/usr/bin/env python3 - "$SARIF_FILE" "$TOP10_CSV" "$ALL_CSV" "$SUMMARY_JSON" "$SUMMARY_MD" <<'PY'
import sys, json, csv, os, datetime, collections
sarif_path, top10_csv, all_csv, summary_json, summary_md = sys.argv[1:6]

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

# Write all findings CSV
with open(all_csv,"w",newline="",encoding="utf-8") as f:
    w=csv.DictWriter(f, fieldnames=["Severity","Rule","Rule ID","File","Line","Message"])
    w.writeheader(); w.writerows(results)

# Top10
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

# Summary JSON & MD
sev_counts = collections.Counter([r["Severity"] for r in results])
summary = {
    "timestamp": datetime.datetime.now().isoformat(),
    "total_findings": len(results),
    "severity_counts": dict(sev_counts),
    "top_rules": [{"severity": sev_, "rule": rule, "count": cnt} for (rule, sev_), cnt in top],
    "artifacts": {
        "sarif": os.path.basename(sarif_path),
        "top10_csv": os.path.basename(top10_csv),
        "all_csv": os.path.basename(all_csv),
    },
}
json.dump(summary, open(summary_json,"w",encoding="utf-8"), ensure_ascii=False, indent=2)

with open(summary_md,"w",encoding="utf-8") as f:
    f.write("# Semgrep Summary\n\n")
    f.write(f"- Total findings: **{summary['total_findings']}**\n")
    if summary["severity_counts"]:
        f.write("- By severity: " + ", ".join(f"{k}: {v}" for k,v in summary["severity_counts"].items()) + "\n")
    f.write("\n## Top rules\n")
    for item in summary["top_rules"]:
        f.write(f"- **{item['severity']}** — {item['rule']} (x{item['count']})\n")
    f.write("\n## Artifacts\n")
    for k,v in summary["artifacts"].items():
        f.write(f"- {k}: `{v}`\n")
print("[DONE] Generated CSV and summary.")
PY

# ---- gating: fail build if requested ----
if [[ -n "$FAIL_ON" ]]; then
  echo "[INFO] Applying fail gate for severities: $FAIL_ON" | tee -a "$LOG_FILE"
  python3 - "$SUMMARY_JSON" "$FAIL_ON" <<'PY'
import sys, json
summary_path, fail_on = sys.argv[1], sys.argv[2]
sev = set([x.strip().upper() for x in fail_on.split(",") if x.strip()])
d = json.load(open(summary_path, encoding="utf-8"))
counts = {k.upper(): int(v) for k,v in d.get("severity_counts",{}).items()}
viol = sum(counts.get(s,0) for s in sev)
print(f"[GATE] Severity counts: {counts} | Fail-on: {sev} | Violations: {viol}")
sys.exit(1 if viol>0 else 0)
PY
fi

echo ""
echo "========================================"
echo " Done. Report dir: ${REPORT_DIR}"
echo " Files:"
echo " - ${SARIF_FILE}"
echo " - ${TOP10_CSV}"
echo " - ${ALL_CSV}"
echo " - ${SUMMARY_JSON}"
echo " - ${SUMMARY_MD}"
echo " Log: ${LOG_FILE}"
echo "========================================"
