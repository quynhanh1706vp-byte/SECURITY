using System;

namespace DeMasterProCloud.DataModel.Visit
{
    public class VisitListHistoryModel
    {
        public int Id { get; set; }

        /// <summary>
        /// visit identifier
        /// </summary>
        public int VisitorId { get; set; }

        /// <summary>
        /// the name of visitor
        /// </summary>
        public string VisitorName { get; set; }

        /// <summary>
        /// previous status code value
        /// </summary>
        public int? OldStatus { get; set; }

        /// <summary>
        /// previous status name value
        /// </summary>
        public string OldStatusString { get; set; }

        /// <summary>
        /// new status code valule
        /// </summary>
        public int NewStatus { get; set; }

        /// <summary>
        /// new status name value
        /// </summary>
        public string NewStatusString { get; set; }

        /// <summary>
        /// identifier of account that updated visitor information
        /// </summary>
        public int UpdatedBy { get; set; }

        /// <summary>
        /// Name value of the account that tried to update
        /// </summary>
        public string UpdatedByName { get; set; }

        /// <summary>
        /// visitor's company identifier
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// datetime when the visitor was created in the system
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// datetime when the visitor was updated
        /// </summary>
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// visitor's card string value
        /// </summary>
        public string CardId { get; set; }


        public DateTime EventTime { get; set; }

        /// <summary>   Gets or sets the type of the event. </summary>
        /// <value> The type of the event. </value>
        public string EventType { get; set; }

        /// <summary>   Gets or sets the event details. </summary>
        /// <value> The event details. </value>
        public string EventDetails { get; set; }
        public string DoorName { get; set; }
        public string Antipass { get; set; }
    }
}