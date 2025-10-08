using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class Card
    {
        public Card()
        {
            FingerPrint = new HashSet<FingerPrint>();
        }
        
        public int Id { get; set; }
        public DateTime? IssuedDate { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime EffectiveDate { get; set; }
        public int CardType { get; set; }
        public int? UserId { get; set; }
        public int? VisitId { get; set; }

        public string CardName { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public short CardStatus { get; set; }
        public string Note { get; set; }

        public int AccessGroupId { get; set; }
        public bool IsMasterCard { get; set; }
        
        public short Status { get; set; }
        public string Etc { get; set; }

        public Company Company { get; set; }
        public User User { get; set; }
        public Visit Visit { get; set; }
        
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        
        public string ManagementNumber { get; set; }
        public short CardRole { get; set; }
        public short CardRoleType { get; set; }
        
        public ICollection<FingerPrint> FingerPrint { get; set; }
    }
}
