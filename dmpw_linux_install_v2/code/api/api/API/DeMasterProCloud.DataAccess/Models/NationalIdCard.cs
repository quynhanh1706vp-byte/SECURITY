using System;

namespace DeMasterProCloud.DataAccess.Models;

public class NationalIdCard
{
    public int Id { get; set; }
    public string CCCD { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Sex { get; set; }
    public string Nationality { get; set; }
    public string Nation { get; set; }
    public string Religion { get; set; }
    public string District { get; set; }
    public string Address { get; set; }
    public string IdentityCharacter { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpiredDate { get; set; }
    public string FatherName { get; set; }
    public string MotherName { get; set; }
    public string HusbandOrWifeName { get; set; }
    public string CMND { get; set; }
    public string Avatar { get; set; }
    
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    
    public int? UserId { get; set; }
    public User? User { get; set; }
    
    public int? VisitId { get; set; }
    public Visit? Visit { get; set; }
}