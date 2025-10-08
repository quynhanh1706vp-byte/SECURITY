#!/usr/bin/env bash
set -euo pipefail

# Usage:
#   ./semgrep_scan.sh [TARGET_DIR] [JOBS]
# Output:
#   - semgrep-results.sarif  (SARIF report)
#   - semgrep-run.log        (full scan logs)

TARGET_DIR="${1:-$PWD}"
JOBS="${2:-4}"

cd "$TARGET_DIR"

LOG_FILE="semgrep-run.log"
SARIF_FILE="semgrep-results.sarif"

echo "[INFO] Scanning: $TARGET_DIR" | tee "$LOG_FILE"
echo "[INFO] Jobs: $JOBS" | tee -a "$LOG_FILE"
echo "[INFO] Logs: $LOG_FILE" | tee -a "$LOG_FILE"
echo "[INFO] SARIF: $SARIF_FILE" | tee -a "$LOG_FILE"

RUN_ARGS=(
  semgrep scan
  --config p/owasp-top-ten
  --config p/javascript
  --config p/csharp
  --config p/python
  --exclude node_modules --exclude dist --exclude build
  --exclude bin --exclude obj --exclude .git
  --exclude '*.pdf' --exclude '*.zip' --exclude '*.tar*'
  --timeout 600 --max-target-bytes 200000000 --jobs "$JOBS"
  --sarif -o "$SARIF_FILE" --error --verbose
)

if command -v docker >/dev/null 2>&1; then
  echo "[INFO] Using Docker image returntocorp/semgrep" | tee -a "$LOG_FILE"
  docker run --rm -v "$PWD":/src -w /src returntocorp/semgrep "${RUN_ARGS[@]}"     |& tee -a "$LOG_FILE"
else
  echo "[INFO] Docker not found; using local 'semgrep' binary" | tee -a "$LOG_FILE"
  semgrep --version | tee -a "$LOG_FILE"
  "${RUN_ARGS[@]}" |& tee -a "$LOG_FILE"
fi

echo "[DONE] Scan complete. See $SARIF_FILE and $LOG_FILE" | tee -a "$LOG_FILE"
