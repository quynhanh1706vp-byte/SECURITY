import re
from pathlib import Path

targets = []
for name in ("startup.cs","program.cs"):
    targets += list(Path(".").rglob(name))

cors_repl = '.WithOrigins("https://app.example.com","https://admin.example.com").AllowAnyHeader().AllowAnyMethod().AllowCredentials()'

for p in targets:
    try:
        text = p.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        continue

    orig = text
    touched = False

    # 1) Siết AllowAnyOrigin
    new = re.sub(r"\.AllowAnyOrigin\s*\(\s*\)", cors_repl, text)
    if new != text:
        text = new
        touched = True

    # 2) Bơm AutoValidateAntiforgeryTokenAttribute cho MVC views nếu có AddControllersWithViews()
    if "AddControllersWithViews(" in text and "AutoValidateAntiforgeryTokenAttribute" not in text:
        text2 = re.sub(
            r"AddControllersWithViews\s*\(\s*\)",
            r"AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))",
            text
        )
        if text2 != text:
            text = text2
            touched = True
            if "using Microsoft.AspNetCore.Mvc;" not in text:
                text = "using Microsoft.AspNetCore.Mvc;\n" + text

    if touched:
        bak = p.with_suffix(p.suffix + ".bak")
        bak.write_text(orig, encoding="utf-8")
        p.write_text(text, encoding="utf-8")
        print(f"Patched: {p}")
