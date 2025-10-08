namespace DeMasterProCloud.DataAccess.Models
{
    public class ShortenLink
    {
        public int Id { get; set; }
        public string FullPath { get; set; }
        public string ShortPath { get; set; }
        public string LocationOrigin { get; set; }
    }
}