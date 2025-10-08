#!/usr/bin/env bash
set -euo pipefail
shopt -s globstar
changed=0
for f in **/*Controller.cs; do
  [[ -f "$f" ]] || continue
  # bỏ qua nếu file đã có [Authorize]
  if grep -q '\[Authorize' "$f"; then continue; fi
  cp "$f" "$f.bak"
  awk '
    BEGIN{ins_using=1; ins_attr=0}
    NR==1 {
      print "using Microsoft.AspNetCore.Authorization;";
      print; next
    }
    ins_attr==0 && $0 ~ /public[[:space:]]+class[[:space:]]+.*Controller[[:space:]]*:/ {
      print "[Authorize]"
      ins_attr=1
    }
    { print }
  ' "$f.bak" > "$f"
  echo "[OK] Added [Authorize] -> $f"
  changed=$((changed+1))
done
echo "Done. Files changed: $changed"
