#!/usr/bin/env bash
set -euo pipefail
TARGET_DIR="${1:-.}"
JOBS="${2:-6}"
METRICS="${METRICS:-on}"          # on/off
TIMEOUT="${TIMEOUT:-1500}"
MAXBYTES="${MAXBYTES:-1500000000}"
REPORT_ROOT="reports/security"
TS="$(date +%Y%m%d_%H%M%S)"
OUT_DIR="$REPORT_ROOT/${TS}_expanded"
API_DIRS=("code/api")
WEB_DIRS=("code/webapp" "install/extracted")

echo "[INFO] Target: $(realpath "$TARGET_DIR")"
echo "[INFO] Report dir: $OUT_DIR"
echo "[INFO] Jobs: $JOBS | Timeout: ${TIMEOUT}s | MaxBytes: ${MAXBYTES}"
mkdir -p "$OUT_DIR"

run_semgrep() {
  local out_sarif="$1"; shift
  echo "[RUN] Semgrep -> $out_sarif"
  docker run --rm -v "$PWD":/src -w /src returntocorp/semgrep semgrep scan     --metrics ${METRICS} --timeout "${TIMEOUT}" --max-target-bytes "${MAXBYTES}" --jobs "${JOBS}"     "$@"     || true  # don't fail the script; we post-process SARIF
  if [[ -s "$out_sarif" ]]; then echo "[OK] SARIF -> $out_sarif"; else echo "[ERR] empty SARIF: $out_sarif"; fi
}

# Pass A: API (C#) deep
API_SARIF="$OUT_DIR/semgrep-api.sarif"
run_semgrep "$API_SARIF"   --config p/r2c-security-audit --config p/owasp-top-ten --config p/secrets   --config p/csharp --config p/csharp-best-practices   --include '**/*.cs' --sarif -o "$API_SARIF"   "${API_DIRS[@]}"

# Pass B: Web/Config (JS/TS/React, Docker, Nginx, secrets)
WEB_SARIF="$OUT_DIR/semgrep-webcfg.sarif"
run_semgrep "$WEB_SARIF"   --config p/javascript --config p/typescript --config p/react   --config p/dockerfile --config p/nginx --config p/secrets   --include '**/*.{js,jsx,ts,tsx,env,conf,yml,yaml,dockerfile}'   --sarif -o "$WEB_SARIF"   "${WEB_DIRS[@]}"

# Pass C: Custom rules (raises risk appropriately)
CUSTOM_SARIF="$OUT_DIR/semgrep-custom.sarif"
run_semgrep "$CUSTOM_SARIF"   --config rules/aspnet_hardening_v1.yml   --include '**/*.{cs,js,jsx,ts,tsx}'   --sarif -o "$CUSTOM_SARIF"   .

# Merge
python3 merge_sarif.py "$OUT_DIR/semgrep-merged.sarif" "$API_SARIF" "$WEB_SARIF" "$CUSTOM_SARIF"

# Convert to CSV/HTML if sarif_quickpack.py is present in CWD; else copy a helper path if available
if [[ -f "sarif_quickpack.py" ]]; then
  python3 sarif_quickpack.py "$OUT_DIR/semgrep-merged.sarif" --outdir "$OUT_DIR"
elif [[ -f "/mnt/data/sarif_quickpack.py" ]]; then
  cp /mnt/data/sarif_quickpack.py "$OUT_DIR/"
  (cd "$OUT_DIR" && python3 sarif_quickpack.py semgrep-merged.sarif --outdir "$OUT_DIR")
fi

echo "========================================"
echo " Done. Report dir: $OUT_DIR"
echo " Artifacts:"
ls -1 "$OUT_DIR" | sed 's/^/  - /'
echo "========================================"
