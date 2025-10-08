# Semgrep Expand v3.3 — API + Web/Config + Custom Rules

## Cách chạy
```bash
cd ~/Data/dmpw_linux_install_v2
chmod +x semgrep_expand_v3_3.sh
./semgrep_expand_v3_3.sh . 6   # 6 jobs
```

- **Pass A**: C# API sâu (`code/api`) — r2c, OWASP, secrets, csharp-best-practices
- **Pass B**: Web/Config (`code/webapp`, `install/extracted`) — JS/TS/React, Dockerfile, Nginx, secrets
- **Pass C**: Custom rules (nâng risk) — `rules/aspnet_hardening_v1.yml`

Kết quả ở `reports/security/<timestamp>_expanded/`:
- `semgrep-api.sarif`, `semgrep-webcfg.sarif`, `semgrep-custom.sarif`, `semgrep-merged.sarif`
- Nếu có `sarif_quickpack.py`: thêm `semgrep-findings-all.csv`, `semgrep-results.top10.csv`, `index.html`, `summary.md`.

## Mẹo
- ENV `METRICS=off` nếu bạn muốn tắt metrics.
- Thêm/điều chỉnh rule trong `rules/aspnet_hardening_v1.yml` (ví dụ đẩy MEDIUM -> HIGH).
