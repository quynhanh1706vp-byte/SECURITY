#!/usr/bin/env bash
set -euo pipefail

mkdir -p reports/security out || true

echo "[1/3] Semgrep scan..."
semgrep scan \
  --config p/ci \
  --config p/r2c-security-audit \
  --config p/secrets \
  --config p/csharp \
  --config p/javascript --config p/typescript --config p/react \
  --config p/dockerfile --config p/iac \
  --config security-advanced.yaml \
  --metrics off \
  --timeout 0 \
  --exclude node_modules --exclude dist --exclude bin --exclude obj \
  --sarif --output reports/security/semgrep_api_custom.sarif \
  2>&1 | tee out/semgrep_scan.log || true

if [ ! -s reports/security/semgrep_api_custom.sarif ]; then
  echo "[ERR] SARIF not created. See out/semgrep_scan.log"
  exit 1
fi
echo "[OK] SARIF ready"

echo "[2/3] Convert SARIF -> CSV/Top10/P1 ..."
python3 - <<'PY'
import json, csv, pathlib, collections
SRC = pathlib.Path("reports/security/semgrep_api_custom.sarif")
OUT = pathlib.Path("reports/security"); OUT.mkdir(parents=True, exist_ok=True)

def sev_map(v):
    v = (v or "").upper()
    return {"BLOCKER":"CRITICAL", "FATAL":"CRITICAL", "HIGH":"ERROR"}.get(v, v or "WARNING")

d = json.load(open(SRC, "r", encoding="utf-8"))
rows = []
for run in d.get("runs", []):
    tool_rules = {}
    for rr in (run.get("tool",{}) or {}).get("driver",{}).get("rules",[]) or []:
        tool_rules[rr.get("id")] = rr
    for r in run.get("results", []) or []:
        rid = (r.get("ruleId") or "").strip()
        msg = (r.get("message") or {}).get("text","")
        lvl = sev_map(r.get("level") or (r.get("properties") or {}).get("severity"))
        file = line = ""
        locs = r.get("locations") or []
        if locs:
            pl  = (locs[0].get("physicalLocation") or {})
            file= (pl.get("artifactLocation") or {}).get("uri","")
            reg = (pl.get("region") or {})
            line= reg.get("startLine") or reg.get("startColumn") or ""
        props = r.get("properties") or {}
        priority = (props.get("priority") or props.get("impact") or "").upper()
        cwe = ""
        tags = props.get("tags") or tool_rules.get(rid,{}).get("properties",{}).get("tags") or []
        if isinstance(tags, list):
            for t in tags:
                if isinstance(t,str) and t.upper().startswith("CWE-"):
                    cwe = t.upper(); break
        rows.append({"Severity":lvl,"Priority":priority,"Rule":rid,"File":file,"Line":line,"Message":msg,"CWE":cwe})

# CSV all
all_csv = OUT / "semgrep_api_custom.csv"
with all_csv.open("w", newline="", encoding="utf-8") as f:
    w = csv.DictWriter(f, fieldnames=["Severity","Priority","Rule","File","Line","Message","CWE"])
    w.writeheader(); w.writerows(rows)

# Top-10
cnt = collections.Counter([r["Rule"] for r in rows])
sev_rank = {"CRITICAL":3,"ERROR":2,"WARNING":1,"INFO":0}
rule2sev, rule2file = {}, {}
for r in rows:
    rule = r["Rule"]; s = r["Severity"]
    if (rule not in rule2sev) or (sev_rank.get(s,0) > sev_rank.get(rule2sev.get(rule,""),0)):
        rule2sev[rule] = s
    rule2file.setdefault(rule, r["File"])
with (OUT/"semgrep_custom_top10.csv").open("w", newline="", encoding="utf-8") as f:
    w = csv.writer(f); w.writerow(["Rank","Rule","Count","RepresentativeSeverity","ExampleFile"])
    for i,(rule,c) in enumerate(cnt.most_common(10),1):
        w.writerow([i,rule,c,rule2sev.get(rule,""),rule2file.get(rule,"")])

# P1 samples: Severity in {ERROR,CRITICAL} or Priority in {P1,HIGH,CRITICAL}
P1_PRI = {"P1","HIGH","CRITICAL"}; P1_SEV = {"CRITICAL","ERROR"}
p1 = [r for r in rows if (r["Priority"] in P1_PRI) or (r["Severity"] in P1_SEV)]
with (OUT/"semgrep_custom_P1_samples.csv").open("w", newline="", encoding="utf-8") as f:
    w = csv.DictWriter(f, fieldnames=["Severity","Priority","Rule","File","Line","Message","CWE"])
    w.writeheader(); w.writerows(p1[:600])

print("[OK] CSVs written")
PY

echo "[3/3] List artifacts:"
ls -lh reports/security | sed -n '1,200p'
