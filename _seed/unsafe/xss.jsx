export default function Bad(){ const html="<img src=x onerror=alert(1)>"; return <div dangerouslySetInnerHTML={{__html: html}}/> }
