namespace DeMasterProCloud.DataAccess.Models
{
    public partial class Setting
    {
        public int Id { get; set; }
        
        public int CompanyId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
