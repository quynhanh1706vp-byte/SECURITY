using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class AttendanceSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        [Column(TypeName = "jsonb")]
        public string ApproverAccounts { get; set; }
        public int TimeFormatId { get; set; }
        public bool EnableNotifyCheckinLate { get; set; }

        public Company Company { get; set; }


        [Column(TypeName = "jsonb")]
        public string InReaders { get; set; }

        [Column(TypeName = "jsonb")]
        public string OutReaders { get; set; }

        public string DayStartTime { get; set; }
    }
}