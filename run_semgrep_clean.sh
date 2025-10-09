#!/usr/bin/env bash
set -euo pipefail

# Nếu chưa cài Semgrep CLI, bạn có thể dùng docker fallback ở cuối file.
OUT="${OUT:-./semgrep.json}"

if command -v semgrep >/dev/null 2>&1; then
  semgrep scan \
    --metrics=on \
    --json --output "$OUT" \
    --exclude '_rules/**' --exclude '_reports/**' --exclude '.semgrepignore' \
    --include 'code/**' --include 'src/**' --include 'app/**' --include 'backend/**' --include 'frontend/**' \
    --config p/ci --config p/csharp --config p/javascript --config p/react --config p/secrets
else
  echo "[INFO] Semgrep CLI chưa có, dùng docker fallback..."
  docker run --rm \
    -v "$PWD":/src -w /src \
    returntocorp/semgrep semgrep scan \
      --metrics=on \
      --json --output "$OUT" \
      --exclude '_rules/**' --exclude '_reports/**' --exclude '.semgrepignore' \
      --include 'code/**' --include 'src/**' --include 'app/**' --include 'backend/**' --include 'frontend/**' \
      --config p/ci --config p/csharp --config p/javascript --config p/react --config p/secrets
fi

echo "Done: $OUT"
