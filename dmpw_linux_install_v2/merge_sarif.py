#!/usr/bin/env python3
import json, sys, pathlib
out = pathlib.Path(sys.argv[1])
inputs = [pathlib.Path(p) for p in sys.argv[2:] if pathlib.Path(p).exists()]
runs = []
for p in inputs:
    try:
        d = json.load(open(p, encoding="utf-8"))
        runs.extend(d.get("runs", []))
    except Exception as e:
        print(f"[WARN] skip {p}: {e}", file=sys.stderr)
doc = {"version":"2.1.0","$schema":"https://json.schemastore.org/sarif-2.1.0.json","runs":runs}
out.write_text(json.dumps(doc, ensure_ascii=False))
print(f"[OK] merged -> {out}")
