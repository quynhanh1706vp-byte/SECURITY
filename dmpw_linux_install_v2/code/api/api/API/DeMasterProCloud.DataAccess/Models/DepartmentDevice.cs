using System;

namespace DeMasterProCloud.DataAccess.Models;

public class DepartmentDevice
{
    public int DepartmentId { get; set; }
    public int IcuId { get; set; }
    public Department Department { get; set; }
    public IcuDevice Icu { get; set; }
        
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
}