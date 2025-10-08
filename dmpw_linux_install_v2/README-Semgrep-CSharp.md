# Semgrep Quick Kit (C# focus)

## 1) Run a focused C# scan (local)
```bash
docker run --rm -v "$PWD":/src -w /src returntocorp/semgrep semgrep scan   --config p/r2c-security-audit --config p/owasp-top-ten --config p/secrets --config p/csharp   --metrics on --timeout 1200 --jobs 6   --include '**/*.cs'   --sarif -o semgrep-results.sarif   code/api
```

## 2) Convert SARIF → CSV/Top10/HTML
```bash
python3 sarif_quickpack.py semgrep-results.sarif
# outputs into reports/security/<timestamp>/*
```

## 3) Use the all-in-one script (targets only; no include/exclude conflicts)
```bash
chmod +x secscan_all_in_one_v3_pro_taint_v3_2a.sh
./secscan_all_in_one_v3_pro_taint_v3_2a.sh . --jobs 6 code/api
# Add 'install/extracted' if you also have code there
```

## 4) (Optional) Enable Semgrep Pro + taint
```bash
semgrep ci login
# The script or docker command will pick up ~/.semgrep and add --pro automatically when applicable.
```

## 5) (Optional) GitHub Actions
Put `semgrep_csharp_actions.yml` under `.github/workflows/` in your repo.
