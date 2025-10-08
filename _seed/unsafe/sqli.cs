using System.Data.SqlClient;
class P { static void Main(string[] a){ var name="admin"; var cmd=new SqlCommand($"select * from Users where name='{name}'"); } }
