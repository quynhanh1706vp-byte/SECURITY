import re
from pathlib import Path

root = Path(".")
changed = 0

class_decl = re.compile(r'^\s*public\s+class\s+(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*(?P<base>ControllerBase|Controller)\b')

for p in root.rglob("*.cs"):
    try:
        lines = p.read_text(encoding="utf-8", errors="ignore").splitlines()
    except Exception:
        continue

    touched = False
    i = 0
    while i < len(lines):
        m = class_decl.match(lines[i])
        if not m:
            i += 1
            continue

        # Thu thập block attributes ngay phía trên (các dòng [] liên tiếp)
        j = i - 1
        attr_start = i
        attrs = []
        while j >= 0 and lines[j].strip().startswith("["):
            attrs.append(lines[j].strip())
            attr_start = j
            j -= 1
        attrs = list(reversed(attrs))

        has_api = any("[ApiController" in a for a in attrs)
        has_authorize = any("[Authorize" in a for a in attrs)
        has_ignore = any("[IgnoreAntiforgeryToken" in a for a in attrs)
        is_api_controller = (m.group("base") == "ControllerBase")

        # Chỉ chèn khi có [ApiController]
        new_attrs = []
        if has_api:
            new_attrs = attrs[:]
            if not has_authorize:
                new_attrs.append("[Authorize]")
            if is_api_controller and not has_ignore:
                new_attrs.append("[IgnoreAntiforgeryToken]")

            if new_attrs != attrs:
                # Ghi đè block attributes
                lines[attr_start:i] = new_attrs
                i = attr_start + len(new_attrs)
                touched = True
                continue

        i += 1

    if touched:
        bak = p.with_suffix(p.suffix + ".bak")
        bak.write_text("\n".join(lines), encoding="utf-8")  # backup nội dung đã vá tạm để tránh mất mát
        # Ghi lại file gốc với nội dung mới
        p.write_text("\n".join(lines), encoding="utf-8")
        changed += 1
        print(f"Patched: {p}")

print(f"Total files patched: {changed}")
