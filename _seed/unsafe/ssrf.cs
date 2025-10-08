using System.Net.Http; class Q{ async System.Threading.Tasks.Task Run(){ var hc=new HttpClient(); var r=await hc.GetAsync("http://169.254.169.254/latest/meta-data"); } }
