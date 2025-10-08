#!/usr/bin/env bash
set -euo pipefail

CONFIG="${1:-semgrep_custom_full.yaml}"
OUTDIR="${2:-./semgrep-report}"
mkdir -p "$OUTDIR"

echo "[*] Running Semgrep with $CONFIG"
semgrep scan --metrics=off --timeout 600 --error \
  --config "$CONFIG" \
  --json --output "$OUTDIR/all.json"

echo "[*] Generating CSVs..."
python3 - <<'PY'
import json, csv, re, os
from pathlib import Path

outdir = Path(os.environ.get("OUTDIR", "semgrep-report"))
data = json.loads((outdir/"all.json").read_text())

groups={}
for r in data.get("results", []):
    groups.setdefault(r["check_id"], []).append(r)

top = sorted(
    ({"rule":k,"severity":v[0]["extra"]["severity"],"count":len(v)} for k,v in groups.items()),
    key=lambda x: x["count"], reverse=True
)[:10]

with open(outdir/"top10.csv","w",newline="",encoding="utf-8") as f:
    w=csv.writer(f); w.writerow(["rule","severity","count"])
    for row in top: w.writerow([row["rule"], row["severity"], row["count"]])

p1={}
for r in data.get("results", []):
    sev=(r.get("extra",{}).get("severity","") or "").upper()
    if sev in {"ERROR","CRITICAL"} and r["check_id"] not in p1:
        msg=re.sub(r"[\\r\\n]+"," ", r["extra"].get("message","")).strip()
        p1[r["check_id"]]=[r["check_id"], sev, r.get("path",""),
                           r.get("start",{}).get("line",""), msg]

with open(outdir/"p1_samples.csv","w",newline="",encoding="utf-8") as f:
    w=csv.writer(f); w.writerow(["rule","severity","path","start_line","message"])
    for row in p1.values(): w.writerow(row)

print("Wrote:", outdir/"top10.csv", outdir/"p1_samples.csv")
PY
echo "[*] Done. See $OUTDIR/"
