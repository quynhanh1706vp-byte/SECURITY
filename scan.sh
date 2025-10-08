#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC_DIR="/src"
UNPACK_DIR="${ROOT_DIR}/dmpw_linux_install_v3/_unpacked"
RULES_FILE="${ROOT_DIR}/_rules/all-custom-pro.yml"
OUT_DIR="${ROOT_DIR}/_reports/semgrep"
IMAGE="returntocorp/semgrep:latest"

mkdir -p "${OUT_DIR}"

echo "ROOT_DIR=${ROOT_DIR}"
echo "UNPACK_DIR=${UNPACK_DIR}"
echo "RULES_FILE=${RULES_FILE}"
echo "OUT_DIR=${OUT_DIR}"

[ -f "${RULES_FILE}" ] || { echo "❌ Missing ${RULES_FILE}"; exit 2; }
[ -d "${UNPACK_DIR}" ] || { echo "❌ Missing ${UNPACK_DIR} (hãy giải nén code vào đây)"; exit 2; }

docker pull ${IMAGE} >/dev/null

# Validate rulepack
docker run --rm -v "${ROOT_DIR}:${SRC_DIR}" -w "${SRC_DIR}" ${IMAGE} \
  semgrep --validate --config "${SRC_DIR}/_rules/all-custom-pro.yml"

# Scan (JSON + SARIF + TEXT)
docker run --rm -t -v "${ROOT_DIR}:${SRC_DIR}" -w "${SRC_DIR}" \
  -e SEMGREP_USE_GIT=off -e SEMGREP_SEND_METRICS=off \
  ${IMAGE} semgrep scan \
    --config p/owasp-top-ten \
    --config p/security-audit \
    --config p/csharp \
    --config p/secrets \
    --config p/docker \
    --config p/kubernetes \
    --config "${SRC_DIR}/_rules/all-custom-pro.yml" \
    "${SRC_DIR}/dmpw_linux_install_v3/_unpacked" \
    --dataflow-traces \
    --no-git-ignore --max-target-bytes 20000000 --timeout 0 \
    --json-output   "${SRC_DIR}/_reports/semgrep/results.json" \
    --sarif-output  "${SRC_DIR}/_reports/semgrep/results.sarif" \
    --text-output   "${SRC_DIR}/_reports/semgrep/results.txt"

echo "✅ Done → ${OUT_DIR}"
