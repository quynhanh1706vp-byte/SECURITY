#!/usr/bin/env bash
set -euo pipefail

OUTDIR="${1:-./semgrep-report}"
shift || true
CONFIGS=("$@")
if [ ${#CONFIGS[@]} -eq 0 ]; then
  echo "Usage: $0 OUTDIR CONFIG1.yaml [CONFIG2.yaml ...]"
  exit 1
fi

mkdir -p "$OUTDIR"

# build chuỗi --config
CFG_ARGS=()
for c in "${CONFIGS[@]}"; do
  CFG_ARGS+=(--config "$c")
done

echo "[1/2] Run Semgrep (Docker) -> JSON"
docker run --rm -v "$PWD":/src -w /src returntocorp/semgrep \
  sh -lc "semgrep scan --metrics=off --timeout 600 --error \
    --include '**/*.cs' --include '**/*.js' --include '**/*.ts' --include '**/*.tsx' \
    --include '**/*.yaml' --include '**/*.yml' --include '**/Dockerfile*' \
    --exclude '.git' --exclude 'node_modules' --exclude 'packages' \
    --exclude 'bin' --exclude 'obj' --exclude 'dist' --exclude 'build' --exclude 'out' \
    ${CFG_ARGS[@]} \
    --json --output '$OUTDIR/all.json' ."

echo "[2/2] Build CSVs"
docker run --rm -v "$PWD":/work -w /work python:3-alpine sh -lc '
python - <<PY
import json, csv, re
from pathlib import Path
outdir=Path("semgrep-report")
data=json.loads((outdir/"all.json").read_text())
# Top10
groups={}
for r in data.get("results",[]): groups.setdefault(r["check_id"],[]).append(r)
top=sorted(({"rule":k,"severity":v[0]["extra"]["severity"],"count":len(v)} for k,v in groups.items()), key=lambda x:x["count"], reverse=True)[:10]
with open(outdir/"top10.csv","w",newline="",encoding="utf-8") as f:
    w=csv.writer(f); w.writerow(["rule","severity","count"]); [w.writerow([t["rule"],t["severity"],t["count"]]) for t in top]
# P1 samples
p1={}
for r in data.get("results",[]):
    sev=(r.get("extra",{}).get("severity","") or "").upper()
    if sev in {"ERROR","CRITICAL"} and r["check_id"] not in p1:
        msg=re.sub(r"[\\r\\n]+"," ", r["extra"].get("message","")).strip()
        p1[r["check_id"]]=[r["check_id"], sev, r.get("path",""), r.get("start",{}).get("line",""), msg]
with open(outdir/"p1_samples.csv","w",newline="",encoding="utf-8") as f:
    w=csv.writer(f); w.writerow(["rule","severity","path","start_line","message"]); [w.writerow(v) for v in p1.values()]
print("Wrote CSVs")
PY
'
echo "[*] Done. See $OUTDIR/"
