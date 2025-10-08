#!/usr/bin/env bash
set -euo pipefail

TARGET_DIR="${1:-.}"
JOBS="${2:-6}"
METRICS="${METRICS:-on}"
TIMEOUT="${TIMEOUT:-1500}"
MAXBYTES="${MAXBYTES:-1500000000}"
REPORT_ROOT="reports/security"
TS="$(date +%Y%m%d_%H%M%S)"
OUT_DIR="$REPORT_ROOT/${TS}_expanded"

API_DIR="code/api"
WEB_DIR1="code/webapp"
WEB_DIR2="install/extracted"

echo "[INFO] Target: $(realpath "$TARGET_DIR")"
echo "[INFO] Report dir: $OUT_DIR"
echo "[INFO] Jobs: $JOBS | Timeout: ${TIMEOUT}s | MaxBytes: ${MAXBYTES}"
mkdir -p "$OUT_DIR"

# helper: run inside repo (C# API, Custom)
run_semgrep_repo() {
  local out_sarif="$1"; shift
  echo "[RUN] Semgrep (repo) -> $out_sarif"
  docker run --rm -v "$PWD":/src -w /src returntocorp/semgrep semgrep scan \
    --metrics ${METRICS} --timeout "${TIMEOUT}" --max-target-bytes "${MAXBYTES}" --jobs "${JOBS}" \
    "$@" || true
  if [[ -s "$out_sarif" ]]; then echo "[OK] SARIF -> $out_sarif"; else echo "[ERR] empty SARIF: $out_sarif"; fi
}

# helper: run outside git for Web/Config (mount read-only sources, write to /out)
run_semgrep_outside_git() {
  local out_sarif_host="$1"; shift
  echo "[RUN] Semgrep (outside git, /scan -> write /out) -> $out_sarif_host"

  # build -v mounts only for existing dirs
  MOUNTS=()
  [[ -d "$WEB_DIR1" ]] && MOUNTS+=( -v "$PWD/$WEB_DIR1":/scan/"$WEB_DIR1":ro )
  [[ -d "$WEB_DIR2" ]] && MOUNTS+=( -v "$PWD/$WEB_DIR2":/scan/"$WEB_DIR2":ro )

  if [[ ${#MOUNTS[@]} -eq 0 ]]; then
    echo "[WARN] No web/config dirs exist. Skipping Web/Config pass."
    : > "$out_sarif_host"; return 0
  fi

  docker run --rm \
    "${MOUNTS[@]}" \
    -v "$PWD/$OUT_DIR":/out \
    -w /scan \
    returntocorp/semgrep semgrep scan \
      --metrics ${METRICS} --timeout "${TIMEOUT}" --max-target-bytes "${MAXBYTES}" --jobs "${JOBS}" \
      --no-git-ignore \
      "$@" \
      --sarif -o /out/semgrep-webcfg.sarif \
      /scan  || true

  if [[ -s "$OUT_DIR/semgrep-webcfg.sarif" ]]; then
    cp "$OUT_DIR/semgrep-webcfg.sarif" "$out_sarif_host"
    echo "[OK] SARIF -> $out_sarif_host"
  else
    echo "[ERR] Web/Config SARIF not created."
    : > "$out_sarif_host"
  fi
}

# Pass A: API (C#)
API_SARIF="$OUT_DIR/semgrep-api.sarif"
run_semgrep_repo "$API_SARIF" \
  --config p/r2c-security-audit --config p/owasp-top-ten --config p/secrets \
  --config p/csharp --config p/csharp-best-practices \
  --include '**/*.cs' \
  --sarif -o "$API_SARIF" \
  "$API_DIR"

# Pass B: Web/Config (docker/nginx/js/ts/react/secrets)
WEBCFG_SARIF="$OUT_DIR/semgrep-webcfg.sarif"
run_semgrep_outside_git "$WEBCFG_SARIF" \
  --config p/javascript --config p/typescript --config p/react \
  --config p/dockerfile --config p/nginx --config p/secrets \
  --include '**/*.{js,jsx,ts,tsx,env,conf,yml,yaml,json}' \
  --include '**/[Dd]ockerfile*'

# Pass C: Custom rules (ASP.NET + FE nâng risk)
CUSTOM_SARIF="$OUT_DIR/semgrep-custom.sarif"
if [[ -f "rules/aspnet_hardening_v1.yml" ]]; then
  run_semgrep_repo "$CUSTOM_SARIF" \
    --config rules/aspnet_hardening_v1.yml \
    --include '**/*.{cs,js,jsx,ts,tsx}' \
    --sarif -o "$CUSTOM_SARIF" \
    "$TARGET_DIR"
else
  echo "[WARN] Custom rules not found: rules/aspnet_hardening_v1.yml (skipped)"
  : > "$CUSTOM_SARIF"
fi

# Merge SARIF
if [[ -f "merge_sarif.py" ]]; then
  python3 merge_sarif.py "$OUT_DIR/semgrep-merged.sarif" "$API_SARIF" "$WEBCFG_SARIF" "$CUSTOM_SARIF"
else
  echo "[WARN] merge_sarif.py not found; skipping merge."
fi

# Build CSV/Top10/HTML nếu có converter
if [[ -f "sarif_quickpack.py" ]]; then
  python3 sarif_quickpack.py "$OUT_DIR/semgrep-merged.sarif" --outdir "$OUT_DIR" || true
fi

echo "========================================"
echo " Done. Report dir: $OUT_DIR"
echo " Artifacts:"
ls -1 "$OUT_DIR" | sed 's/^/  - /'
echo "========================================"
