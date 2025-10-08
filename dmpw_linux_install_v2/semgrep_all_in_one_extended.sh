  #!/usr/bin/env bash
  set -euo pipefail

  TARGET_ROOT="${1:-.}"
  JOBS="${2:-6}"
  OUT_DIR="${3:-reports/security/$(date -u +%Y%m%d_%H%M%S)}"

  echo "[INFO] Target: ${TARGET_ROOT}"
  echo "[INFO] Jobs: ${JOBS}"
  echo "[INFO] Out dir: ${OUT_DIR}"
  mkdir -p "${OUT_DIR}"

  # Decide whether to use Pro (if ~/.semgrep exists in host and we mount it)
  PRO_FLAG="${SEMGR_PRO_FLAG:-}"
  if [ -d "${HOME}/.semgrep" ]; then
    PRO_FLAG="--pro --config p/ci"
    echo "[INFO] Using Semgrep Pro (detected ~/.semgrep)."
  else
    echo "[INFO] Using community rules only."
  fi

  docker run --rm -v "$PWD":/src -w /src -v "$HOME/.semgrep":/root/.semgrep     returntocorp/semgrep semgrep scan       ${PRO_FLAG}       --config p/r2c-security-audit       --config p/csharp --config p/javascript --config p/typescript --config p/react       --config p/secrets --config p/dockerfile --config p/iac       --config rules/       --include '**/*.{cs,cshtml,ts,tsx,js,json,yml,yaml,env,conf}'       --include '**/[Dd]ockerfile*'       --exclude node_modules --exclude dist --exclude build --exclude coverage       --exclude bin --exclude obj --exclude .git --exclude '*.min.*'       --timeout 1800 --jobs "${JOBS}" --metrics on       --sarif -o "${OUT_DIR}/semgrep_all.sarif"       "${TARGET_ROOT}" |& tee "${OUT_DIR}/semgrep-run.log"

  # Optional: quick CSV extract from SARIF (Rule,Severity,File,Line,Message)
  python3 - <<'PY' "${OUT_DIR}/semgrep_all.sarif" "${OUT_DIR}/semgrep-findings.csv"
import sys, json, csv, pathlib
sarif = pathlib.Path(sys.argv[1])
out = pathlib.Path(sys.argv[2])
data = json.load(open(sarif, "r", encoding="utf-8"))
rows=[]
for run in data.get("runs", []):
    for r in run.get("results", []) or []:
        rid = r.get("ruleId") or ""
        msg = (r.get("message") or {}).get("text","")
        lvl = (r.get("level") or r.get("properties",{}).get("severity") or "UNKNOWN").upper()
        file=""; line=""
        locs = r.get("locations") or []
        if locs:
            pl = (locs[0].get("physicalLocation") or {})
            file = (pl.get("artifactLocation") or {}).get("uri","")
            line = (pl.get("region") or {}).get("startLine","")
        rows.append({"Severity":lvl,"Rule":rid,"File":file,"Line":line,"Message":msg})
with out.open("w", newline="", encoding="utf-8") as f:
    w = csv.DictWriter(f, fieldnames=["Severity","Rule","File","Line","Message"])
    w.writeheader(); w.writerows(rows)
print(f"[OK] CSV: {out}")
PY

  echo "========================================"
  echo " Done. Report dir: ${OUT_DIR}"
  echo " Artifacts:"
  echo "  - semgrep_all.sarif"
  echo "  - semgrep-run.log"
  echo "  - semgrep-findings.csv"
  echo "========================================"
