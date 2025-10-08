#!/usr/bin/env python3
import json, sys, csv
from pathlib import Path
from collections import Counter

def normalize_severity(level: str) -> str:
    lvl = (level or "").lower()
    if lvl in ("error", "high"): return "HIGH"
    if lvl in ("warning", "medium"): return "MEDIUM"
    if lvl in ("note", "low"): return "LOW"
    return "INFO"

def main():
    if len(sys.argv) < 2:
        print("Usage: python sarif_top10.py <semgrep-results.sarif> [out.csv]")
        sys.exit(2)
    sarif_path = Path(sys.argv[1])
    out_csv = Path(sys.argv[2]) if len(sys.argv) > 2 else sarif_path.with_suffix(".top10.csv")

    data = json.loads(sarif_path.read_text(encoding="utf-8"))
    runs = data.get("runs", [])
    results = []
    for run in runs:
        rules = {r.get("id"): r for r in run.get("tool", {}).get("driver", {}).get("rules", [])}
        for r in run.get("results", []):
            rid = r.get("ruleId", "")
            rr = rules.get(rid, {})
            level = r.get("level") or rr.get("defaultConfiguration", {}).get("level") or rr.get("properties", {}).get("problem.severity") or ""
            sev = normalize_severity(level)
            name = rr.get("shortDescription", {}).get("text") or rr.get("name") or rid or "Unknown"
            locs = r.get("locations", [])
            file_uri, line = "", ""
            if locs:
                phys = locs[0].get("physicalLocation", {})
                file_uri = phys.get("artifactLocation", {}).get("uri", "")
                line = phys.get("region", {}).get("startLine", "")
            msg = r.get("message", {}).get("text", "")[:160]
            results.append({"severity": sev, "rule": name, "file": file_uri, "line": line, "msg": msg})

    sev_order = {"HIGH":0,"MEDIUM":1,"LOW":2,"INFO":3}
    # Count per (rule, severity)
    from collections import defaultdict
    counter = Counter((x["rule"], x["severity"]) for x in results)
    top = sorted(counter.items(), key=lambda kv: (sev_order.get(kv[0][1], 9), -kv[1], kv[0][0]))[:10]

    rows = []
    for (rule, sev), cnt in top:
        samples = [x for x in results if x["rule"] == rule and x["severity"] == sev][:3]
        for i, s in enumerate(samples, 1):
            rows.append({
                "Severity": sev,
                "Rule": rule,
                "Count": cnt if i == 1 else "",
                "Sample File": s["file"],
                "Line": s["line"],
                "Message": s["msg"]
            })

    with open(out_csv, "w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=["Severity","Rule","Count","Sample File","Line","Message"])
        w.writeheader()
        for r in rows:
            w.writerow(r)

    print(f"[DONE] Wrote Top 10 to {out_csv}")

if __name__ == "__main__":
    main()
