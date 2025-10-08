import re, sys
from pathlib import Path

if len(sys.argv) < 2:
    print("Usage: python tools/strip_secrets.py <path/to/Constants.cs>")
    sys.exit(1)

p = Path(sys.argv[1])
src = p.read_text(encoding="utf-8", errors="ignore")
orig = src

# Đảm bảo có using System;
if "using System;" not in src:
    src = "using System;\n" + src

# Regex bắt const string: public const string Name = "value";
pat = re.compile(
    r'(public\s+const\s+string\s+)'
    r'([A-Za-z0-9_]+)'
    r'(\s*=\s*)'
    r'"([^"]*)"'
    r'\s*;',
    re.MULTILINE
)

# Heuristics nhận diện secret
name_secret = re.compile(r'(key|token|secret|password|passwd|connectionstring|apikey)$', re.IGNORECASE)
looks_base64 = re.compile(r'^[A-Za-z0-9+/=]{24,}$')
looks_hex    = re.compile(r'^[0-9A-Fa-f]{32,}$')
looks_jwt    = re.compile(r'^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$')

def is_secret(name: str, value: str) -> bool:
    # Bỏ qua JSON/role template dài
    if "[{" in value or "}]" in value or value.strip().startswith("{"):
        return False
    if name_secret.search(name):
        return True
    if looks_jwt.match(value):
        return True
    if looks_base64.match(value):
        return True
    if looks_hex.match(value):
        return True
    # số dài (ít gặp) -> có thể là id/phone -> bỏ qua
    return False

def to_env_name(name: str) -> str:
    return "APP_" + re.sub(r'[^A-Za-z0-9_]', '_', name).upper()

count = 0
def repl(m):
    global count
    prefix, name, eq, value = m.groups()
    if is_secret(name, value):
        count += 1
        env = to_env_name(name)
        # chuyển const -> static property => ENV
        return f'public static string {name} => Environment.GetEnvironmentVariable("{env}") ?? "";'
    else:
        return m.group(0)

new = pat.sub(repl, src)

if new != orig:
    bak = p.with_suffix(p.suffix + ".bak")
    bak.write_text(orig, encoding="utf-8")
    p.write_text(new, encoding="utf-8")
    print(f"Patched {p} — replaced {count} secret const(s). Backup: {bak.name}")
else:
    print("No eligible const-string secrets found (or already patched).")
