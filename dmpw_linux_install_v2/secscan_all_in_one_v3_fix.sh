\
#!/usr/bin/env bash
set -euo pipefail

# secscan_all_in_one_v3_fix.sh — Hotfix: safe --include handling
# See: --include needs an argument -> only add includes if the directory exists.

TARGET_DIR="${1:-.}"; shift || true

JOBS="${JOBS:-}"
TIMEOUT=1200
MAX_BYTES=1000000000
USE_AUTO=1
USE_AUDIT=1
USE_SECRETS=1
USE_DOCKERFILE=1
USE_IAC=1
RESPECT_GITIGNORE=0
EXTRACT=1
FAIL_ON="HIGH,CRITICAL"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --jobs) JOBS="${2:-}"; shift 2;;
    --no-extract) EXTRACT=0; shift;;
    --respect-gitignore) RESPECT_GITIGNORE=1; shift;;
    *) shift;;
  esac
done

cd "$TARGET_DIR"
if [[ -z "${JOBS}" ]]; then
  if command -v nproc >/dev/null 2>&1; then JOBS="$(nproc)"; else JOBS=4; fi
fi

TS="$(date +%Y%m%d_%H%M%S)"
REPORT_DIR="reports/security/${TS}"
EXTRACT_DIR="${REPORT_DIR}/extracted"
mkdir -p "${REPORT_DIR}"
LOG_FILE="${REPORT_DIR}/00_master.log"
SEMSARIF="${REPORT_DIR}/semgrep-results.sarif"

echo "[INFO] Target: $PWD" | tee -a "$LOG_FILE"
echo "[INFO] Report dir: ${REPORT_DIR}" | tee -a "$LOG_FILE"

if [[ $EXTRACT -eq 1 ]]; then
  mkdir -p "$EXTRACT_DIR"
  shopt -s nullglob
  for gz in code/*.tar.gz code/**/*.tar.gz install/*.tar.gz install/**/*.tar.gz *.tar.gz; do
    base="$(basename "$gz" .tar.gz)"
    dest="${EXTRACT_DIR}/${base}"
    mkdir -p "$dest"
    echo "[INFO] Extract: $gz -> $dest" | tee -a "$LOG_FILE"
    tar -xzf "$gz" -C "$dest" || true
  done
  shopt -u nullglob
fi

INCLUDES=()
# Add only existing directories
[[ -d "$EXTRACT_DIR" ]] && INCLUDES+=( "$EXTRACT_DIR/**" )
[[ -d "code" ]] && INCLUDES+=( "code/**" )
[[ -d "install/extracted" ]] && INCLUDES+=( "install/extracted/**" )
INCLUDES+=( "." )

DOCKER_RUN=( docker run --rm -u "$(id -u)":"$(id -g)" -v "$PWD":/src -w /src )
SCAN_ARGS=(
  semgrep scan
  --config p/owasp-top-ten
  --config p/r2c-security-audit
  --config p/secrets
  --config p/javascript --config p/typescript --config p/python --config p/csharp
  --config p/dockerfile  --config auto
  --timeout "${TIMEOUT}" --max-target-bytes "${MAX_BYTES}" --jobs "${JOBS}" --metrics on
  --sarif -o "/src/${SEMSARIF}" --error --verbose
)
# Respect or ignore .gitignore
[[ $RESPECT_GITIGNORE -eq 0 ]] && SCAN_ARGS+=( --no-git-ignore )

# Append includes safely
for inc in "${INCLUDES[@]}"; do
  SCAN_ARGS+=( --include "$inc" )
done
# Excludes
for exc in node_modules dist build bin obj .git '*.tar*' '*.zip' '*.pdf'; do
  SCAN_ARGS+=( --exclude "$exc" )
done

echo "[RUN] Semgrep scan…" | tee -a "$LOG_FILE"
"${DOCKER_RUN[@]}" returntocorp/semgrep "${SCAN_ARGS[@]}" |& tee -a "${LOG_FILE}"

echo "[OK] SARIF -> ${SEMSARIF}"
