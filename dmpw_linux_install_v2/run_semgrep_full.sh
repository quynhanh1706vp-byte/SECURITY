#!/usr/bin/env bash
set -euo pipefail

JOBS="${JOBS:-8}"
OUT="reports/security/semgrep_all.sarif"

docker run --rm -v "$PWD":/src -w /src -v "$HOME/.semgrep":/root/.semgrep \
  returntocorp/semgrep sh -lc '
    git config --global --add safe.directory /src || true;
    semgrep scan \
      --pro --config p/ci \
      --config p/r2c-security-audit \
      --config p/csharp --config p/javascript --config p/typescript --config p/react \
      --config p/secrets --config p/dockerfile --config p/iac \
      --config semgrep_extended_pack/rules/ \
      --no-git-ignore \
      --include "code/api/**/*.cs" \
      --include "code/webapp/**/*.{ts,tsx,js}" \
      --include "install/extracted/**/*.{yml,yaml,json,env,conf}" \
      --include "install/extracted/**/[Dd]ockerfile*" \
      --exclude node_modules --exclude dist --exclude build --exclude coverage \
      --exclude bin --exclude obj --exclude .git --exclude "*.min.*" \
      --max-target-bytes 5000000 \
      --timeout 1800 --jobs '"$JOBS"' --metrics on \
      --sarif -o '"$OUT"' \
      .
  ' |& tee reports/security/semgrep-run.log
