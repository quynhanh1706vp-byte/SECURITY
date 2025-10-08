#!/usr/bin/env bash
set -euo pipefail
shopt -s globstar
changed=0
for f in **/*Controller.cs; do
  [[ -f "$f" ]] || continue
  # Có [ApiController] và chưa có IgnoreAntiforgery?
  if grep -q '\[ApiController\]' "$f" && ! grep -q '\[IgnoreAntiforgeryToken\]' "$f"; then
    cp "$f" "$f.bak"
    awk '
      BEGIN{ins_using=1; ins_attr=0}
      NR==1 {
        # đảm bảo using cần thiết có mặt
        print "using Microsoft.AspNetCore.Antiforgery;";
        print; next
      }
      ins_attr==0 && $0 ~ /public[[:space:]]+class[[:space:]]+.*Controller[[:space:]]*:/ {
        print "[IgnoreAntiforgeryToken]"
        ins_attr=1
      }
      { print }
    ' "$f.bak" > "$f"
    echo "[OK] Added [IgnoreAntiforgeryToken] -> $f"
    changed=$((changed+1))
  fi
done
echo "Done. Files changed: $changed"
