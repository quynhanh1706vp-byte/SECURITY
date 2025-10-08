using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public class PlugIn
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        
        public string PlugIns { get; set; }
        
        public string PlugInsDescription { get; set; }
        
        public virtual Company Company { get; set; }
    }
}