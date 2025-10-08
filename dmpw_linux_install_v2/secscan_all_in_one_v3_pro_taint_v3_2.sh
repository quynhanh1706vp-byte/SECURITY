\
#!/usr/bin/env bash
set -euo pipefail

# secscan_all_in_one_v3_pro_taint_v3_2.sh — Semgrep Pro + taint (resilient)
# Improvements vs v3.1:
#  - Treat some non-zero exit codes as warnings if SARIF is produced
#  - Cleaner default includes; avoid "both include and exclude" warning
#  - Optional .semgrepignore bootstrap for noisy dirs
#  - Prints quick triage (counts by severity, top rules)
#  - Keeps --pro after 'semgrep scan' (correct) and no --error (gating decides)
#
# Usage example:
#   ./secscan_all_in_one_v3_pro_taint_v3_2.sh . --jobs 6 --include 'code/**' --include 'install/extracted/**' \
#      --fail-on HIGH,CRITICAL --metrics on

TARGET_DIR="${1:-.}"; shift || true

JOBS="${JOBS:-}"
TIMEOUT=1500
MAX_BYTES=1500000000
PRO=1
AUTO=1
METRICS="on"
RESPECT_GITIGNORE=0
INCLUDE_PATTERNS=()
EXCLUDE_PATTERNS=( "node_modules" "dist" "build" "bin" "obj" ".git" "*.tar*" "*.zip" "*.pdf" "reports/**" "semgrep_extracted/**" )
FAIL_ON=""
BOOTSTRAP_IGNORE=1  # create .semgrepignore if missing

while [[ $# -gt 0 ]]; do
  case "$1" in
    --jobs) JOBS="${2:-}"; shift 2;;
    --timeout) TIMEOUT="${2:-1500}"; shift 2;;
    --max-bytes) MAX_BYTES="${2:-1500000000}"; shift 2;;
    --include) INCLUDE_PATTERNS+=( "$2" ); shift 2;;
    --exclude) EXCLUDE_PATTERNS+=( "$2" ); shift 2;;
    --metrics) METRICS="${2:-on}"; shift 2;;
    --fail-on) FAIL_ON="${2:-}"; shift 2;;
    --pro) PRO=1; shift;;
    --no-pro) PRO=0; shift;;
    --auto) AUTO=1; shift;;
    --no-auto) AUTO=0; shift;;
    --respect-gitignore) RESPECT_GITIGNORE=1; shift;;
    --no-bootstrap-ignore) BOOTSTRAP_IGNORE=0; shift;;
    *) echo "[WARN] Unknown option: $1"; shift;;
  esac
done

command -v docker >/dev/null 2>&1 || { echo "[ERROR] Docker is required."; exit 2; }
cd "$TARGET_DIR"
[[ -z "${JOBS}" ]] && { command -v nproc >/dev/null && JOBS="$(nproc)" || JOBS=4; }

TS="$(date +%Y%m%d_%H%M%S)"
REPORT_DIR="reports/security/${TS}"
mkdir -p "${REPORT_DIR}"
LOG_FILE="${REPORT_DIR}/semgrep-run.log"
SARIF="${REPORT_DIR}/semgrep-results.sarif"
TOP10="${REPORT_DIR}/semgrep-results.top10.csv"
ALLCSV="${REPORT_DIR}/semgrep-findings-all.csv"

echo "[INFO] Target: $PWD"
echo "[INFO] Report dir: ${REPORT_DIR}"
echo "[INFO] Jobs: ${JOBS} | Timeout: ${TIMEOUT}s | MaxBytes: ${MAX_BYTES}"
echo "[INFO] Pro: ${PRO} | Auto: ${AUTO} | Metrics: ${METRICS} | Respect .gitignore: ${RESPECT_GITIGNORE}"
echo "[INFO] Fail gate: ${FAIL_ON:-NONE}"

# Bootstrap .semgrepignore (avoid scanning reports/ and extracted artifacts)
if [[ $BOOTSTRAP_IGNORE -eq 1 && ! -f ".semgrepignore" ]]; then
  cat > .semgrepignore <<'IGN'
# auto-generated to reduce noise
reports/**
semgrep_extracted/**
**/*.min.js
**/vendor/**
**/third_party/**
IGN
  echo "[INFO] Wrote .semgrepignore (you can adjust later)"
fi

PROXY_ENV=()
for v in http_proxy https_proxy no_proxy HTTP_PROXY HTTPS_PROXY NO_PROXY; do
  [[ -n "${!v-}" ]] && PROXY_ENV+=( -e "$v=${!v}" )
done

UIDGID="$(id -u):$(id -g)"
SEMG_IMG="returntocorp/semgrep"

# Default includes: prefer code dirs; fall back to '.' if none provided
INCLUDES=()
[[ -d "code" ]] && INCLUDES+=( "code/**" )
[[ -d "install/extracted" ]] && INCLUDES+=( "install/extracted/**" )
[[ ${#INCLUDES[@]} -eq 0 ]] && INCLUDES+=( "." )
for inc in "${INCLUDE_PATTERNS[@]:-}"; do INCLUDES+=( "$inc" ); done

# Base args
SCAN_ARGS=( semgrep scan
  --config p/owasp-top-ten
  --config p/r2c-security-audit
  --config p/secrets
  --config p/dockerfile
  --config p/iac
  --config p/javascript --config p/typescript --config p/python --config p/csharp
)

[[ "${METRICS}" != "on" ]] && AUTO=0
[[ $AUTO -eq 1 ]] && SCAN_ARGS+=( --config auto )

SCAN_ARGS_PRO=( "${SCAN_ARGS[@]}" )
[[ $PRO -eq 1 ]] && SCAN_ARGS_PRO+=( --pro )

for inc in "${INCLUDES[@]}";  do SCAN_ARGS+=( --include "$inc" ); SCAN_ARGS_PRO+=( --include "$inc" ); done
for exc in "${EXCLUDE_PATTERNS[@]}"; do SCAN_ARGS+=( --exclude "$exc" ); SCAN_ARGS_PRO+=( --exclude "$exc" ); done
[[ $RESPECT_GITIGNORE -eq 0 ]] && { SCAN_ARGS+=( --no-git-ignore ); SCAN_ARGS_PRO+=( --no-git-ignore ); }
[[ "${METRICS}" == "on" ]] && { SCAN_ARGS+=( --metrics on ); SCAN_ARGS_PRO+=( --metrics on ); } || { SCAN_ARGS+=( --metrics off ); SCAN_ARGS_PRO+=( --metrics off ); }

COMMON=( --timeout "${TIMEOUT}" --max-target-bytes "${MAX_BYTES}" --jobs "${JOBS}" --sarif -o "/src/${SARIF}" --verbose )
SCAN_ARGS+=( "${COMMON[@]}" ); SCAN_ARGS_PRO+=( "${COMMON[@]}" )

DOCKER_RUN=( docker run --rm -u "${UIDGID}" -v "$PWD":/src -w /src "${PROXY_ENV[@]}" )
[[ -d "$HOME/.semgrep" ]] && DOCKER_RUN+=( -v "$HOME/.semgrep":/home/semgrep/.semgrep )

run_and_maybe_continue () {
  set +e
  "${DOCKER_RUN[@]}" "${SEMG_IMG}" "$@" |& tee -a "${LOG_FILE}"
  local rc=$?
  set -e
  # If SARIF exists and non-empty, allow continue even if rc != 0
  if [[ -s "${SARIF}" ]]; then
    echo "[INFO] Semgrep returned rc=${rc} but SARIF exists; continuing…" | tee -a "${LOG_FILE}"
    return 0
  fi
  return ${rc}
}

echo "[RUN] Semgrep (attempt 1) — Pro=${PRO}, Auto=${AUTO}" | tee -a "${LOG_FILE}"
if ! run_and_maybe_continue "${SCAN_ARGS_PRO[@]}"; then
  echo "[WARN] Attempt 1 failed & no SARIF created. Retrying without --pro/--auto…" | tee -a "${LOG_FILE}"
  if ! run_and_maybe_continue "${SCAN_ARGS[@]}"; then
    echo "[ERROR] Semgrep failed and no SARIF produced. See ${LOG_FILE}."
    exit 1
  fi
fi

/usr/bin/env python3 - "${SARIF}" "${TOP10}" "${ALLCSV}" <<'PY'
import sys, json, csv, os, html, collections
sarif, top10_csv, all_csv = sys.argv[1:4]
if not os.path.exists(sarif) or os.path.getsize(sarif) < 3:
    print("[POST] No SARIF to process."); sys.exit(0)
def sev(level):
    l=(level or "").lower()
    if l in ("critical","error","high"): return "HIGH"
    if l in ("warning","medium"): return "MEDIUM"
    if l in ("note","low"): return "LOW"
    return "INFO"
d=json.load(open(sarif,encoding="utf-8"))
rows=[]; rules={}
for run in d.get("runs",[]):
    rules={r.get("id"):r for r in (run.get("tool",{}) .get("driver",{}) .get("rules",[]) or [])}
    for r in run.get("results",[]):
        rid=r.get("ruleId",""); rr=rules.get(rid,{})
        lvl=r.get("level") or (rr.get("defaultConfiguration",{}) or {}).get("level") or (rr.get("properties",{}) or {}).get("problem.severity")
        name=(rr.get("shortDescription",{}) or {}).get("text") or rr.get("name") or rid or "Unknown"
        msg=(r.get("message",{}) or {}).get("text","")
        for loc in r.get("locations",[]) or [{}]:
            phys=(loc.get("physicalLocation",{}) or {})
            file=(phys.get("artifactLocation",{}) or {}).get("uri","")
            line=(phys.get("region",{}) or {}).get("startLine","")
            rows.append({"Severity":sev(lvl),"Rule":name,"Rule ID":rid,"File":file,"Line":line,"Message":msg})
# full CSV
with open(all_csv,"w",newline="",encoding="utf-8") as f:
    w=csv.DictWriter(f,fieldnames=["Severity","Rule","Rule ID","File","Line","Message"]); w.writeheader(); w.writerows(rows)
# top-10
from collections import Counter
order={"HIGH":0,"MEDIUM":1,"LOW":2,"INFO":3}
counter=Counter((r["Rule"], r["Severity"]) for r in rows)
top=sorted(counter.items(), key=lambda kv:(order.get(kv[0][1],9), -kv[1], kv[0][0]))[:10]
rows_top=[["Severity","Rule","Count","Sample File","Line","Message"]]
for (rule, sev_), cnt in top:
    samples=[r for r in rows if r["Rule"]==rule and r["Severity"]==sev_][:3]
    for i,s in enumerate(samples,1):
        rows_top.append([sev_, rule, cnt if i==1 else "", s["File"], s["Line"], (s["Message"] or "")[:200]])
with open(top10_csv,"w",newline="",encoding="utf-8") as f:
    csv.writer(f).writerows(rows_top)
# quick triage
sev_counts=Counter([r["Severity"] for r in rows])
print("[TRIAGE] Counts by severity:", dict(sev_counts))
from collections import defaultdict
by_rule=defaultdict(int)
for r in rows: by_rule[(r["Rule"], r["Severity"])] += 1
print("[TRIAGE] Top rules:", sorted(by_rule.items(), key=lambda kv:-kv[1])[:5])
PY

# Gating
if [[ -n "${FAIL_ON}" ]]; then
  echo "[GATE] Checking fail-on severities: ${FAIL_ON}"
  if [[ -s "${ALLCSV}" ]]; then
    FAIL_SEVS="$(echo "${FAIL_ON}" | tr '[:lower:]' '[:upper:]' | tr ',' ' ')"
    TOTAL=0
    for s in ${FAIL_SEVS}; do
      C=$( (tail -n +2 "${ALLCSV}" | cut -d',' -f1 | grep -c -E "^${s}$") || true )
      echo "  - ${s}: ${C}"; TOTAL=$((TOTAL + C))
    done
    if [[ $TOTAL -gt 0 ]]; then echo "[GATE] Violations=${TOTAL} → FAIL"; exit 1; else echo "[GATE] OK"; fi
  else
    echo "[GATE] No findings file to check."
  fi
else
  echo "[GATE] No gating set."
fi

echo ""
echo "========================================"
echo " Done. Report dir: ${REPORT_DIR}"
echo " Artifacts:"
ls -1 "${REPORT_DIR}" | sed 's/^/  - /'
echo " Log: ${LOG_FILE}"
echo "========================================"
