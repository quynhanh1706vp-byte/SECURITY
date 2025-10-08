#!/usr/bin/env bash
set -euo pipefail

CONFIG="${1:-semgrep_custom_pro.yaml}"
OUTDIR="${2:-./semgrep-report}"
mkdir -p "$OUTDIR"

echo "[*] Running Semgrep with $CONFIG"
semgrep scan \
  --metrics=off \
  --timeout 600 \
  --config "$CONFIG" \
  --config p/csharp --config p/javascript --config p/typescript \
  --config p/secrets --config p/dockerfile --config p/iac \
  --error \
  --json --output "$OUTDIR/semgrep-findings.json"

semgrep scan \
  --metrics=off \
  --timeout 600 \
  --config "$CONFIG" \
  --config p/csharp --config p/javascript --config p/typescript \
  --config p/secrets --config p/dockerfile --config p/iac \
  --error \
  --sarif --output "$OUTDIR/semgrep-findings.sarif"

echo "[*] Done. See $OUTDIR/"
