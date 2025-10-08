#!/usr/bin/env bash
set -euo pipefail

# secscan_all_in_one_v4.sh — Ultra One-shot AppSec Scanner
# SAST + Secrets + SBOM/SCA + IaC + HTML Dashboard + Baseline Diff + CI Gate

TARGET_DIR="${1:-.}"; shift || true
JOBS="${JOBS:-}"
TIMEOUT=1500
MAX_BYTES=1500000000
USE_PRO=0
PROFILE="full"
USE_AUTO=1; USE_AUDIT=1; USE_SECRETS=1; USE_DOCKERFILE=1; USE_IAC=1
USE_TRIVY=1; USE_CHECKOV=1; USE_TFSEC=0; USE_KUBELINTER=0; USE_TRUFFLEHOG=0; USE_OSV=1
RESPECT_GITIGNORE=0
EXTRACT=1
FAIL_ON=""
METRICS="on"
INCLUDE_PATTERNS=()
EXCLUDE_PATTERNS=( "node_modules" "dist" "build" "bin" "obj" ".git" "*.tar*" "*.zip" "*.pdf" )
GIT_HISTORY=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --pro) USE_PRO=1; shift;;
    --full) PROFILE="full"; shift;;
    --ci-fast) PROFILE="ci-fast"; USE_TRIVY=0; USE_CHECKOV=0; USE_TFSEC=0; USE_KUBELINTER=0; shift;;
    --auto) USE_AUTO=1; shift;;
    --no-auto) USE_AUTO=0; shift;;
    --audit) USE_AUDIT=1; shift;;
    --no-audit) USE_AUDIT=0; shift;;
    --secrets) USE_SECRETS=1; shift;;
    --no-secrets) USE_SECRETS=0; shift;;
    --dockerfile) USE_DOCKERFILE=1; shift;;
    --no-dockerfile) USE_DOCKERFILE=0; shift;;
    --iac) USE_IAC=1; shift;;
    --no-iac) USE_IAC=0; shift;;
    --trivy) USE_TRIVY=1; shift;;
    --no-trivy) USE_TRIVY=0; shift;;
    --checkov) USE_CHECKOV=1; shift;;
    --no-checkov) USE_CHECKOV=0; shift;;
    --tfsec) USE_TFSEC=1; shift;;
    --no-tfsec) USE_TFSEC=0; shift;;
    --kube-linter) USE_KUBELINTER=1; shift;;
    --no-kube-linter) USE_KUBELINTER=0; shift;;
    --trufflehog) USE_TRUFFLEHOG=1; shift;;
    --no-trufflehog) USE_TRUFFLEHOG=0; shift;;
    --osv) USE_OSV=1; shift;;
    --no-osv) USE_OSV=0; shift;;
    --git-history) GIT_HISTORY=1; shift;;
    --respect-gitignore) RESPECT_GITIGNORE=1; shift;;
    --no-extract) EXTRACT=0; shift;;
    --jobs) JOBS="${2:-}"; shift 2;;
    --timeout) TIMEOUT="${2:-1500}"; shift 2;;
    --max-bytes) MAX_BYTES="${2:-1500000000}"; shift 2;;
    --include) INCLUDE_PATTERNS+=( "$2" ); shift 2;;
    --exclude) EXCLUDE_PATTERNS+=( "$2" ); shift 2;;
    --fail-on) FAIL_ON="${2:-}"; shift 2;;
    --metrics) METRICS="${2:-on}"; shift 2;;
    *) echo "[WARN] Unknown option: $1"; shift;;
  esac
done

if ! command -v docker >/dev/null 2>&1; then
  echo "[ERROR] Docker is required."; exit 2
fi
cd "$TARGET_DIR"
if [[ -z "${JOBS}" ]]; then if command -v nproc >/dev/null 2>&1; then JOBS="$(nproc)"; else JOBS=4; fi; fi

TS="$(date +%Y%m%d_%H%M%S)"
REPORT_DIR="reports/security/${TS}"
EXTRACT_DIR="${REPORT_DIR}/extracted"
mkdir -p "${REPORT_DIR}"
LOG_FILE="${REPORT_DIR}/00_master.log"

echo "[INFO] Target: $PWD" | tee -a "$LOG_FILE"
echo "[INFO] Report dir: ${REPORT_DIR}" | tee -a "$LOG_FILE"
echo "[INFO] Jobs: ${JOBS}" | tee -a "$LOG_FILE"
echo "[INFO] Profile: ${PROFILE}" | tee -a "$LOG_FILE"
echo "[INFO] Metrics: ${METRICS}" | tee -a "$LOG_FILE"

if [[ "${METRICS}" != "on" && ${USE_AUTO} -eq 1 ]]; then
  echo "[WARN] auto-config requires metrics ON. Disabling auto." | tee -a "$LOG_FILE"
  USE_AUTO=0
fi

# Extract
if [[ $EXTRACT -eq 1 ]]; then
  mkdir -p "$EXTRACT_DIR"; shopt -s nullglob
  for gz in code/*.tar.gz code/**/*.tar.gz install/*.tar.gz install/**/*.tar.gz *.tar.gz; do
    base="$(basename "$gz" .tar.gz)"; dest="${EXTRACT_DIR}/${base}"; mkdir -p "$dest"
    echo "[INFO] Extract: $gz -> $dest" | tee -a "$LOG_FILE"
    tar -xzf "$gz" -C "$dest" || true
  done
  shopt -u nullglob
fi

UIDGID="$(id -u):$(id -g)"
SEMG_IMG="returntocorp/semgrep"; GLKS_IMG="zricethezav/gitleaks:latest"
SYFT_IMG="anchore/syft:latest"; GRYPE_IMG="anchore/grype:latest"
TRIVY_IMG="aquasec/trivy:latest"; CHKOV_IMG="bridgecrew/checkov:latest"
TFSEC_IMG="aquasec/tfsec:latest"; KUBEL_IMG="stackrox/kube-linter:latest"
TRFHOG_IMG="trufflesecurity/trufflehog:latest"; OSV_IMG="ghcr.io/google/osv-scanner:latest"

INCLUDES=()
[[ -d "$EXTRACT_DIR" ]] && INCLUDES+=( "$EXTRACT_DIR/**" )
[[ -d "code" ]] && INCLUDES+=( "code/**" )
[[ -d "install/extracted" ]] && INCLUDES+=( "install/extracted/**" )
INCLUDES+=( "." )
for inc in "${INCLUDE_PATTERNS[@]:-}"; do INCLUDES+=( "$inc" ); done

# 1) Semgrep
SEMLOG="${REPORT_DIR}/semgrep-run.log"
SEMSARIF="${REPORT_DIR}/semgrep-results.sarif"
SEMTOP10="${REPORT_DIR}/semgrep-results.top10.csv"
SEMALL="${REPORT_DIR}/semgrep-findings-all.csv"
echo "[RUN] Semgrep…" | tee -a "$LOG_FILE"
DOCKER_RUN=( docker run --rm -u "${UIDGID}" -v "$PWD":/src -w /src )
if [[ $USE_PRO -eq 1 && -d "$HOME/.semgrep" ]]; then
  DOCKER_RUN+=( -v "$HOME/.semgrep":/home/semgrep/.semgrep )
fi
SCAN_ARGS=( semgrep scan --config p/owasp-top-ten )
[[ $USE_AUDIT -eq 1 ]]   && SCAN_ARGS+=( --config p/r2c-security-audit )
[[ $USE_SECRETS -eq 1 ]] && SCAN_ARGS+=( --config p/secrets )
SCAN_ARGS+=( --config p/javascript --config p/typescript --config p/python --config p/csharp )
[[ $USE_DOCKERFILE -eq 1 ]] && SCAN_ARGS+=( --config p/dockerfile )
[[ $USE_IAC -eq 1 ]]        && SCAN_ARGS+=( --config p/iac )
[[ $USE_AUTO -eq 1 ]]       && SCAN_ARGS+=( --config auto )
for inc in "${INCLUDES[@]}"; do SCAN_ARGS+=( --include "$inc" ); done
for exc in "${EXCLUDE_PATTERNS[@]}"; do SCAN_ARGS+=( --exclude "$exc" ); done
[[ $RESPECT_GITIGNORE -eq 0 ]] && SCAN_ARGS+=( --no-git-ignore )
[[ "${METRICS}" == "on" ]] && SCAN_ARGS+=( --metrics on ) || SCAN_ARGS+=( --metrics off )
SCAN_ARGS+=( --timeout "${TIMEOUT}" --max-target-bytes "${MAX_BYTES}" --jobs "${JOBS}" )
SCAN_ARGS+=( --sarif -o "/src/${SEMSARIF}" --error --verbose )
"${DOCKER_RUN[@]}" "${SEMG_IMG}" "${SCAN_ARGS[@]}" |& tee "${SEMLOG}" || true

/usr/bin/env python3 - "$SEMSARIF" "$SEMTOP10" "$SEMALL" <<'PY'
import sys, json, csv, os
sarif, top10_csv, all_csv = sys.argv[1:4]
if not os.path.exists(sarif): sys.exit(0)
def sev(level):
    l=(level or "").lower()
    if l in ("critical","error","high"): return "HIGH"
    if l in ("warning","medium"): return "MEDIUM"
    if l in ("note","low"): return "LOW"
    return "INFO"
d=json.load(open(sarif,encoding="utf-8"))
rows=[]; rules={}
for run in d.get("runs",[]):
    rules={r.get("id"):r for r in (run.get("tool",{}) .get("driver",{}) .get("rules",[]) or [])}
    for r in run.get("results",[]):
        rid=r.get("ruleId",""); rr=rules.get(rid,{})
        lvl=r.get("level") or (rr.get("defaultConfiguration",{}) or {}).get("level") or (rr.get("properties",{}) or {}).get("problem.severity")
        name=(rr.get("shortDescription",{}) or {}).get("text") or rr.get("name") or rid or "Unknown"
        msg=(r.get("message",{}) or {}).get("text","")
        for loc in r.get("locations",[]) or [{}]:
            phys=(loc.get("physicalLocation",{}) or {})
            file=(phys.get("artifactLocation",{}) or {}).get("uri","")
            line=(phys.get("region",{}) or {}).get("startLine","")
            rows.append({"Tool":"Semgrep","Severity":sev(lvl),"Rule":name,"Rule ID":rid,"File":file,"Line":line,"Message":msg})
with open(all_csv,"w",newline="",encoding="utf-8") as f:
    w=csv.DictWriter(f,fieldnames=["Tool","Severity","Rule","Rule ID","File","Line","Message"]); w.writeheader(); w.writerows(rows)
from collections import Counter
order={"HIGH":0,"MEDIUM":1,"LOW":2,"INFO":3}
counter=Counter((r["Rule"], r["Severity"]) for r in rows)
top=sorted(counter.items(), key=lambda kv:(order.get(kv[0][1],9), -kv[1], kv[0][0]))[:10]
rows_top=[["Severity","Rule","Count","Sample File","Line","Message"]]
for (rule, sev_), cnt in top:
    samples=[r for r in rows if r["Rule"]==rule and r["Severity"]==sev_][:3]
    for i,s in enumerate(samples,1):
        rows_top.append([sev_, rule, cnt if i==1 else "", s["File"], s["Line"], (s["Message"] or "")[:200]])
with open(top10_csv,"w",newline="",encoding="utf-8") as f: csv.writer(f).writerows(rows_top)
PY

# 2) Gitleaks FS + optional Git history
echo "[RUN] Gitleaks (filesystem) …" | tee -a "$LOG_FILE"
GLK_SARIF="${REPORT_DIR}/gitleaks.sarif"
docker run --rm -u "${UIDGID}" -v "$PWD":/repo "${GLKS_IMG}" detect \
  --no-git -s /repo -f sarif -r /repo/"${GLK_SARIF}" |& tee -a "$LOG_FILE" || true
if [[ $GIT_HISTORY -eq 1 && -d .git ]]; then
  echo "[RUN] Gitleaks (git history) …" | tee -a "$LOG_FILE"
  GLK_GIT_SARIF="${REPORT_DIR}/gitleaks-git.sarif"
  docker run --rm -u "${UIDGID}" -v "$PWD":/repo "${GLKS_IMG}" detect \
    -s /repo -f sarif -r /repo/"${GLK_GIT_SARIF}" |& tee -a "$LOG_FILE" || true
fi

# 3) TruffleHog (optional)
if [[ $USE_TRUFFLEHOG -eq 1 ]]; then
  echo "[RUN] TruffleHog …" | tee -a "$LOG_FILE"
  TRFHOG_SARIF="${REPORT_DIR}/trufflehog.sarif"
  docker run --rm -u "${UIDGID}" -v "$PWD":/path trufflesecurity/trufflehog:latest filesystem /path --format sarif > "${TRFHOG_SARIF}" || true
fi

# 4) SBOM + SCA
echo "[RUN] Syft (SBOM) & Grype (SCA) …" | tee -a "$LOG_FILE"
SBOM="${REPORT_DIR}/sbom-cdx.json"; GRYPE_SARIF="${REPORT_DIR}/sca-grype.sarif"
docker run --rm -u "${UIDGID}" -v "$PWD":/src anchore/syft:latest dir:/src -o cyclonedx-json -q > "${SBOM}" || true
docker run --rm -u "${UIDGID}" -v "$PWD":/src anchore/grype:latest sbom:/src/"${SBOM}" -o sarif -q > "${GRYPE_SARIF}" || true

# 5) OSV-Scanner
if [[ ${USE_OSV} -eq 1 ]]; then
  echo "[RUN] OSV-Scanner …" | tee -a "$LOG_FILE"
  OSV_SARIF="${REPORT_DIR}/osv.sarif"
  docker run --rm -u "${UIDGID}" -v "$PWD":/src ghcr.io/google/osv-scanner:latest --format sarif --output /src/"${OSV_SARIF}" /src |& tee -a "$LOG_FILE" || true
fi

# 6) Trivy FS
if [[ ${USE_TRIVY} -eq 1 ]]; then
  echo "[RUN] Trivy FS …" | tee -a "$LOG_FILE"
  TRIVY_SARIF="${REPORT_DIR}/trivy-fs.sarif"
  docker run --rm -u "${UIDGID}" -v "$PWD":/src aquasec/trivy:latest fs /src \
    --scanners vuln,secret,misconfig --skip-dirs node_modules,dist,build,bin,obj,.git \
    --format sarif --output /src/"${TRIVY_SARIF}" --timeout 10m |& tee -a "$LOG_FILE" || true
fi

# 7) IaC: Checkov + tfsec + KubeLinter
if [[ ${USE_IAC} -eq 1 && ${USE_CHECKOV} -eq 1 ]]; then
  echo "[RUN] Checkov …" | tee -a "$LOG_FILE"
  CHK_SARIF="${REPORT_DIR}/checkov.sarif"
  docker run --rm -u "${UIDGID}" -v "$PWD":/src bridgecrew/checkov:latest -d /src -o sarif --output-file-path /src/"${CHK_SARIF}" |& tee -a "$LOG_FILE" || true
fi
if [[ ${USE_IAC} -eq 1 && ${USE_TFSEC} -eq 1 ]]; then
  echo "[RUN] tfsec …" | tee -a "$LOG_FILE"
  TFSEC_SARIF="${REPORT_DIR}/tfsec.sarif"
  docker run --rm -u "${UIDGID}" -v "$PWD":/src aquasec/tfsec:latest /src --format sarif --out /src/"${TFSEC_SARIF}" |& tee -a "$LOG_FILE" || true
fi
if [[ ${USE_KUBELINTER} -eq 1 ]]; then
  echo "[RUN] KubeLinter …" | tee -a "$LOG_FILE"
  KUBE_SARIF="${REPORT_DIR}/kube-linter.sarif"
  docker run --rm -u "${UIDGID}" -v "$PWD":/src stackrox/kube-linter:latest lint /src --format sarif > "${KUBE_SARIF}" || true
fi

# 8) Aggregate + summary + diff + HTML
/usr/bin/env python3 - "${REPORT_DIR}" "${FAIL_ON}" <<'PY'
import sys, json, csv, os, glob, datetime, collections, html, pathlib
report_dir, fail_on = sys.argv[1], (sys.argv[2] or "")
sarif_files = glob.glob(os.path.join(report_dir, "*.sarif"))
def norm_sev(level, props):
    if not level and props:
        level = props.get("security-severity") or props.get("problem.severity") or ""
    l=(level or "").lower()
    if l in ("critical",): return "CRITICAL"
    if l in ("error","high"): return "HIGH"
    if l in ("warning","medium"): return "MEDIUM"
    if l in ("note","low"): return "LOW"
    return "INFO"
rows=[]
for path in sarif_files:
    data=json.load(open(path,encoding="utf-8"))
    tool="unknown"
    for run in data.get("runs",[]):
        drv=(run.get("tool",{}) or {}).get("driver",{}) or {}
        tool = drv.get("name") or tool
        rules={r.get("id"): r for r in (drv.get("rules",[]) or [])}
        for r in run.get("results",[]):
            rid=r.get("ruleId",""); rr=rules.get(rid,{})
            level = r.get("level") or (rr.get("defaultConfiguration",{}) or {}).get("level") or ""
            props = r.get("properties") or rr.get("properties",{}) or {}
            severity = norm_sev(level, props)
            name = (rr.get("shortDescription",{}) or {}).get("text") or rr.get("name") or rid or "Unknown"
            msg  = (r.get("message",{}) or {}).get("text","")
            for loc in r.get("locations",[]) or [{}]:
                phys=(loc.get("physicalLocation",{}) or {})
                file=(phys.get("artifactLocation",{}) or {}).get("uri","")
                line=(phys.get("region",{}) or {}).get("startLine","")
                rows.append({"Tool":tool,"Severity":severity,"Rule":name,"Rule ID":rid,"File":file,"Line":line,"Message":msg[:400]})
# write combined
combined_csv = os.path.join(report_dir, "combined-findings.csv")
with open(combined_csv,"w",newline="",encoding="utf-8") as f:
    w=csv.DictWriter(f, fieldnames=["Tool","Severity","Rule","Rule ID","File","Line","Message"])
    w.writeheader(); w.writerows(rows)
sev_counts = collections.Counter([r["Severity"] for r in rows])
by_tool = collections.Counter([r["Tool"] for r in rows])
summary = {
    "timestamp": datetime.datetime.now().isoformat(),
    "sarif_files": [os.path.basename(x) for x in sarif_files],
    "total_findings": len(rows),
    "severity_counts": dict(sev_counts),
    "by_tool": dict(by_tool),
}
json.dump(summary, open(os.path.join(report_dir,"summary.json"),"w",encoding="utf-8"), ensure_ascii=False, indent=2)
with open(os.path.join(report_dir,"summary.md"),"w",encoding="utf-8") as f:
    f.write("# Security Scan Summary\n\n")
    f.write(f"- Total findings: **{summary['total_findings']}**\n")
    if summary["severity_counts"]:
        f.write("- By severity: " + ", ".join(f"{k}: {v}" for k,v in summary["severity_counts"].items()) + "\n")
    if summary["by_tool"]:
        f.write("- By tool: " + ", ".join(f"{k}: {v}" for k,v in summary["by_tool"].items()) + "\n")
    f.write("\nArtifacts:\n")
    for s in summary["sarif_files"]:
        f.write(f"- {s}\n")
# baseline diff
cur = pathlib.Path(report_dir); root = cur.parent
others = sorted([p for p in root.glob("*") if p.is_dir() and p.name < cur.name])
if others:
    prev = others[-1]
    prev_csv = prev / "combined-findings.csv"
    if prev_csv.exists():
        import csv as _csv
        def key(r): return (r["Tool"], r["Rule"], r["File"], str(r["Line"]), r["Severity"])
        def load_rows(p):
            return [row for row in _csv.DictReader(open(p,encoding="utf-8"))]
        cur_rows = load_rows(combined_csv); prev_rows = load_rows(prev_csv.as_posix())
        cur_set = {key(r):r for r in cur_rows}; prev_set = {key(r):r for r in prev_rows}
        new_keys = cur_set.keys() - prev_set.keys()
        resolved_keys = prev_set.keys() - cur_set.keys()
        with open(os.path.join(report_dir,"baseline_diff.md"),"w",encoding="utf-8") as f:
            f.write("# Baseline diff\n\n")
            f.write(f"- New findings: **{len(new_keys)}**\n")
            f.write(f"- Resolved since last run: **{len(resolved_keys)}**\n\n")
            f.write("## Sample of new (up to 20)\n")
            for k in list(new_keys)[:20]:
                r=cur_set[k]; f.write(f"- [{r['Severity']}] {r['Tool']} — {r['Rule']} @ {r['File']}:{r['Line']}\n")
            f.write("\n## Sample of resolved (up to 20)\n")
            for k in list(resolved_keys)[:20]:
                r=prev_set[k]; f.write(f"- [{r['Severity']}] {r['Tool']} — {r['Rule']} @ {r['File']}:{r['Line']}\n")
# HTML mini dashboard
html_path = os.path.join(report_dir,"index.html")
def esc(s): 
    import html as _h; return _h.escape(str(s) if s is not None else "")
with open(html_path,"w",encoding="utf-8") as h:
    h.write("<!doctype html><meta charset='utf-8'><title>Security Scan Dashboard</title>")
    h.write("<style>body{font-family:system-ui,Segoe UI,Arial;padding:20px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ddd;padding:6px}th{background:#f4f4f4;text-align:left}</style>")
    h.write("<h1>Security Scan Dashboard</h1>")
    h.write(f"<p><b>Total findings:</b> {summary['total_findings']}<br>")
    h.write("<b>By severity:</b> " + ", ".join(f"{k}:{v}" for k,v in summary['severity_counts'].items()) + "<br>")
    h.write("<b>By tool:</b> " + ", ".join(f"{k}:{v}" for k,v in summary['by_tool'].items()) + "</p>")
    h.write("<details open><summary>Combined findings (first 500 rows)</summary>")
    h.write("<table><tr><th>Tool</th><th>Severity</th><th>Rule</th><th>File</th><th>Line</th><th>Message</th></tr>")
    for r in rows[:500]:
        h.write("<tr><td>{}</td><td>{}</td><td>{}</td><td>{}</td><td>{}</td><td>{}</td></tr>".format(
            esc(r['Tool']), esc(r['Severity']), esc(r['Rule']), esc(r['File']), esc(r['Line']), esc(r['Message'])
        ))
    h.write("</table></details>")
    h.write("<h3>Artifacts</h3><ul>")
    for s in sorted(summary["sarif_files"]): h.write(f"<li>{esc(s)}</li>")
    h.write("</ul>")
# gating
sev_counts = collections.Counter([r["Severity"] for r in rows])
if fail_on:
    sevset=set(x.strip().upper() for x in fail_on.split(",") if x.strip())
    viol=sum(sev_counts.get(s,0) for s in sevset)
    print(f"[GATE] Fail-on {sevset} -> Violations: {viol}")
    sys.exit(1 if viol>0 else 0)
else:
    print("[GATE] No gating set.")
PY

echo ""
echo "========================================"
echo " Done. Report dir: ${REPORT_DIR}"
echo " Open HTML dashboard: ${REPORT_DIR}/index.html"
echo " Artifacts include:"
ls -1 "${REPORT_DIR}" | sed 's/^/  - /'
echo " Log: ${LOG_FILE}"
echo "========================================"
