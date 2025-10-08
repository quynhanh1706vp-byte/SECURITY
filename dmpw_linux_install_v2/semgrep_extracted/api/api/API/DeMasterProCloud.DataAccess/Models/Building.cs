using DeMasterProCloud.Common.Infrastructure;
using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    /// <summary>
    /// Building
    /// </summary>
    public partial class Building
    {
        public Building(string timeZone = null)
        {
            IcuDevice = new HashSet<IcuDevice>();
            InverseParent = new HashSet<Building>();

            TimeZone = string.IsNullOrEmpty(timeZone) ? Helpers.GetLocalTimeZone() : timeZone;
        }

        /// <summary>
        /// Company ID of the building.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// index of building in DB
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Building name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// index of account that made the building
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// the time the building was made
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// index of account that updated the building the latest
        /// </summary>
        public int UpdatedBy { get; set; }

        /// <summary>
        /// the time the building was updated the latest
        /// </summary>
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Deletion status
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// company of the building
        /// </summary>
        public Company Company { get; set; }

        /// <summary>
        /// building address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// city of building address
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// country of building address
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// postal code of building address
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// location of building address
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// timezone of building.\n
        /// This is set according to address.
        /// </summary>
        public string TimeZone { get; set; }
        
        public int? ParentId { get; set; }
        
        public Building Parent { get; set; }
        public ICollection<Building> InverseParent { get; set; }

        /// <summary>
        /// list of device in the building
        /// </summary>
        public ICollection<IcuDevice> IcuDevice { get; set; }
        
    }
}