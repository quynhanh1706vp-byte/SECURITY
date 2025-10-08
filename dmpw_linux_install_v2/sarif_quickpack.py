
#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import argparse, json, csv, os, sys, datetime, html
from pathlib import Path
from collections import Counter

def sev(level: str) -> str:
    l = (level or "").lower()
    if l in ("critical","error","high"): return "HIGH"
    if l in ("warning","medium"): return "MEDIUM"
    if l in ("note","low"): return "LOW"
    return "INFO"

def main():
    ap = argparse.ArgumentParser(description="Convert Semgrep SARIF to CSV/Top10/Markdown")
    ap.add_argument("sarif", help="Path to semgrep-results.sarif")
    ap.add_argument("--outdir", default=None, help="Output dir (default: reports/security/<timestamp>)")
    ap.add_argument("--max", type=int, default=500, help="Max findings to show in HTML (default 500)")
    args = ap.parse_args()

    sarif_path = Path(args.sarif)
    if not sarif_path.exists():
        print(f"[ERR] SARIF not found: {sarif_path}", file=sys.stderr); sys.exit(2)

    ts = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    outdir = Path(args.outdir) if args.outdir else Path("reports/security")/ts
    outdir.mkdir(parents=True, exist_ok=True)

    with sarif_path.open(encoding="utf-8") as f:
        d = json.load(f)

    rows = []
    rules = {}
    for run in d.get("runs",[]):
        driver = (run.get("tool",{}) or {}).get("driver",{}) or {}
        for r in driver.get("rules",[]) or []:
            rules[r.get("id")] = r
        for r in run.get("results",[]) or []:
            rid = r.get("ruleId","")
            rr  = rules.get(rid,{})
            lvl = r.get("level") or (rr.get("defaultConfiguration",{}) or {}).get("level") or (rr.get("properties",{}) or {}).get("problem.severity")
            name = (rr.get("shortDescription",{}) or {}).get("text") or rr.get("name") or rid or "Unknown"
            msg  = (r.get("message",{}) or {}).get("text","")
            fixes = (r.get("fixes") or [])
            fix   = ""
            if fixes:
                # SARIF fixes are arrays of artifacts/changes; we keep a short note
                fix = "Has SARIF fix suggestion"
            locs = r.get("locations") or [{}]
            for loc in locs:
                phys = (loc.get("physicalLocation",{}) or {})
                file = (phys.get("artifactLocation",{}) or {}).get("uri","")
                line = (phys.get("region",{}) or {}).get("startLine","")
                rows.append({
                    "Severity": sev(lvl),
                    "Rule": name,
                    "Rule ID": rid,
                    "File": file,
                    "Line": line,
                    "Message": msg,
                    "FixHint": fix
                })

    # Write full CSV
    full_csv = outdir/"semgrep-findings-all.csv"
    with full_csv.open("w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=["Severity","Rule","Rule ID","File","Line","Message","FixHint"])
        w.writeheader(); w.writerows(rows)

    # Top10 CSV
    order={"HIGH":0,"MEDIUM":1,"LOW":2,"INFO":3}
    counter = Counter((r["Rule"], r["Severity"]) for r in rows)
    top = sorted(counter.items(), key=lambda kv:(order.get(kv[0][1],9), -kv[1], kv[0][0]))[:10]
    top_csv = outdir/"semgrep-results.top10.csv"
    with top_csv.open("w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(["Severity","Rule","Count","Sample File","Line","Message"])
        for (rule, sev_), cnt in top:
            samples=[r for r in rows if r["Rule"]==rule and r["Severity"]==sev_][:3]
            for i,s in enumerate(samples,1):
                w.writerow([sev_, rule, cnt if i==1 else "", s["File"], s["Line"], (s["Message"] or "")[:200]])

    # Markdown summary
    md = outdir/"summary.md"
    sev_counts = Counter(r["Severity"] for r in rows)
    with md.open("w", encoding="utf-8") as f:
        f.write("# Semgrep Summary\n\n")
        f.write(f"- SARIF: `{sarif_path}`\n")
        f.write(f"- Total findings: **{len(rows)}**\n")
        f.write("- By severity: " + ", ".join(f"{k}: {v}" for k,v in sorted(sev_counts.items())) + "\n")
        f.write("- Artifacts: `semgrep-findings-all.csv`, `semgrep-results.top10.csv`\n")

    # Minimal HTML dashboard
    html_path = outdir/"index.html"
    def esc(x): return html.escape(str(x) if x is not None else "")
    with html_path.open("w", encoding="utf-8") as h:
        h.write("<!doctype html><meta charset='utf-8'><title>Semgrep Dashboard</title>")
        h.write("<style>body{font-family:system-ui,Segoe UI,Arial;padding:20px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ddd;padding:6px}th{background:#f4f4f4;text-align:left}</style>")
        h.write("<h1>Semgrep Dashboard</h1>")
        h.write(f"<p><b>Total findings:</b> {len(rows)}<br>")
        h.write("<b>By severity:</b> " + ", ".join(f"{k}:{v}" for k,v in sorted(sev_counts.items())) + "</p>")
        h.write("<details open><summary>First {} findings</summary><table>".format(min(args.max, len(rows))))
        h.write("<tr><th>Severity</th><th>Rule</th><th>File</th><th>Line</th><th>Message</th></tr>")
        for r in rows[:args.max]:
            h.write("<tr><td>{}</td><td>{}</td><td>{}</td><td>{}</td><td>{}</td></tr>".format(
                esc(r["Severity"]), esc(r["Rule"]), esc(r["File"]), esc(r["Line"]), esc(r["Message"])
            ))
        h.write("</table></details>")

    print("[OK] Wrote:")
    for p in [full_csv, top_csv, md, html_path]:
        print(" -", p)

if __name__ == "__main__":
    main()
