using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class AccessSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        [Column(TypeName = "jsonb")]
        public string FirstApproverAccounts { get; set; }
        [Column(TypeName = "jsonb")]
        public string SecondApproverAccounts { get; set; }
        public int ApprovalStepNumber{ get; set; }
        public bool EnableAutoApproval { get; set; }
        public bool AllowDeleteRecord { get; set; }
        public string AllLocationWarning { get; set; }
        public int DeviceIdCheckIn { get; set; }
        public string ListFieldsEnable { get; set; }
        public string VisibleFields { get; set; }
        public Company Company { get; set; }
    }
}