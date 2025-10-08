
#!/usr/bin/env bash
set -euo pipefail
# openapi_fix_ckv20_21.sh — Fix CKV_OPENAPI_20 (JWT apiKey -> http/bearer) & CKV_OPENAPI_21 (add maxItems)
# Usage:
#   ./openapi_fix_ckv20_21.sh <swagger.json|dir> [--recursive] [--in-place] [--max-items N]
# Examples:
#   ./openapi_fix_ckv20_21.sh code/webapp/swagger.json --in-place --max-items 1000
#   ./openapi_fix_ckv20_21.sh . --recursive --in-place --max-items 500
# Requires: jq

TARGET="${1:-}"
shift || true
RECURSIVE=0
INPLACE=0
MAX_ITEMS=1000

while [[ $# -gt 0 ]]; do
  case "$1" in
    --recursive) RECURSIVE=1; shift;;
    --in-place|--inplace) INPLACE=1; shift;;
    --max-items) MAX_ITEMS="${2:-1000}"; shift 2;;
    *) echo "[WARN] Unknown option: $1"; shift;;
  esac
done

if ! command -v jq >/dev/null 2>&1; then
  echo "[ERROR] Need 'jq'. Try: sudo apt-get update && sudo apt-get install -y jq"
  exit 2
fi

if [[ -z "${TARGET}" ]]; then
  echo "Usage: $0 <swagger.json|dir> [--recursive] [--in-place] [--max-items N]" >&2
  exit 2
fi

FILES=()
if [[ -f "${TARGET}" ]]; then
  FILES+=("${TARGET}")
elif [[ -d "${TARGET}" ]]; then
  if [[ ${RECURSIVE} -eq 1 ]]; then
    while IFS= read -r -d '' f; do FILES+=("$f"); done < <(find "${TARGET}" -type f -name "swagger.json" -print0)
  else
    [[ -f "${TARGET}/swagger.json" ]] && FILES+=("${TARGET}/swagger.json")
  fi
else
  echo "[ERROR] Not found: ${TARGET}" >&2
  exit 2
fi

if [[ ${#FILES[@]} -eq 0 ]]; then
  echo "[INFO] No swagger.json found in '${TARGET}' (check path or use --recursive)." >&2
  exit 0
fi

echo "[INFO] MAX_ITEMS=${MAX_ITEMS}  INPLACE=${INPLACE}  FILES=${#FILES[@]}"

JQ_PROG='
def walk(f):
  . as $in
  | if type == "object" then
      reduce keys[] as $key ( {}; . + { ($key): ($in[$key] | walk(f)) } ) | f
    elif type == "array" then
      map( walk(f) ) | f
    else
      f
    end;

# Security schemes: add bearerAuth, drop JWT; set global security to bearerAuth
. as $root
| .components = (.components // {})
| .components.securitySchemes = (.components.securitySchemes // {})
| .components.securitySchemes
  |= ( del(.JWT)
     | .bearerAuth = { "type": "http", "scheme": "bearer", "bearerFormat": "JWT" }
     )
| .security = [ { "bearerAuth": [] } ]

# Replace any operation-level security { "JWT": [...] } -> { "bearerAuth": [...] }
| walk(
    if (type=="object" and has("JWT") and (.JWT|type=="array")) then
      .bearerAuth = .JWT | del(.JWT)
    else . end
  )

# servers: force https:// if http://
| .servers = ( (.servers // []) | map(
    if (.url? | type == "string") and (.url | test("^http://")) then
      .url |= sub("^http://"; "https://")
    else . end
  ))

# Add maxItems to any array schema that lacks it
| walk(
    if (type=="object" and (.type? == "array") and ((.maxItems? // null) == null)) then
      .maxItems = (env.MAX_ITEMS | tonumber? // 1000)
    else . end
  )
'

for f in "${FILES[@]}"; do
  echo "[FIX] ${f}"
  if [[ ${INPLACE} -eq 1 ]]; then
    cp -a "${f}" "${f}.bak"
    MAX_ITEMS="${MAX_ITEMS}" jq "${JQ_PROG}" "${f}" > "${f}.tmp"
    mv -f "${f}.tmp" "${f}"
    echo "  -> updated (backup: ${f}.bak)"
  else
    MAX_ITEMS="${MAX_ITEMS}" jq "${JQ_PROG}" "${f}" > "${f}.fixed"
    echo "  -> wrote ${f}.fixed (dry-run)"
  fi
done

echo "[DONE] All swagger.json processed."
