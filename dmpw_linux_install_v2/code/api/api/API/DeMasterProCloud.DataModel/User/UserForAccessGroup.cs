using DeMasterProCloud.DataModel.Category;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.User
{
    public class UserForAccessGroup
    {
        //public int Id { get; set; }
        //public string CardId { get; set; }
        //public string FullName { get; set; }
        //public string FullName { get; set; }
        //public string DepartmentName { get; set; }
        //public List<CardModel> CardList { get; set; }


        public int Id { get; set; }
        public string Avatar { get; set; }

        /// <summary>
        /// User's name.
        /// </summary>
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        public string UserCode { get; set; }

        /// <summary>
        /// Department name
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// Access group name
        /// </summary>
        public string AccessGroupName { get; set; }

        /// <summary>
        /// Employee number
        /// This is for normal (non army) user.
        /// </summary>
        public string EmployeeNo { get; set; }
        
        /// <summary>
        /// User's position
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// User's expired date
        /// </summary>
        public string ExpiredDate { get; set; }

        /// <summary>
        /// The name of user's work type.
        /// </summary>
        public string WorkTypeName { get; set; }

        /// <summary>
        /// List of cards
        /// </summary>
        public List<CardModel> CardList { get; set; }

        /// <summary>
        /// List of vehicle
        /// </summary>
        public List<CardModel> PlateNumberList { get; set; }

        /// <summary>
        /// List of category data
        /// </summary>
        public List<UserCategoryDataModel> CategoryOptions { get; set; }

        /// <summary>
        /// Approval status
        /// </summary>
        public string ApprovalStatus { get; set; }
    }

    public class UnAssignUserForAccessGroup
    {
        public int Id { get; set; }
        public string CardId { get; set; }
        
        /// <summary>
        /// User's name.
        /// </summary>
        public string FullName { get; set; }
        public string FirstName { get; set; }

        /// <summary>
        /// Department name
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// User's position
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Access group name
        /// </summary>
        public string AccessGroupName { get; set; }

        /// <summary>
        /// Employee number
        /// This is for normal (non army) user.
        /// </summary>
        public string EmployeeNo { get; set; }
        
        /// <summary>
        /// List of cards
        /// </summary>
        public List<CardModel> CardList { get; set; }

        /// <summary>
        /// List of category data
        /// </summary>
        public List<UserCategoryDataModel> CategoryOptions { get; set; }

    }
}
