namespace DeMasterProCloud.DataAccess.Models
{
    public class FingerPrint
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public string Templates { get; set; }
        
        public int CardId { get; set; }
        public Card Card { get; set; }
    }
}