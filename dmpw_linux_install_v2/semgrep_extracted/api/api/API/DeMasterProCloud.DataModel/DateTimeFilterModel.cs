using DeMasterProCloud.Common.Infrastructure;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.DataModel
{
    public class DateTimeFilterModel
    {
        private readonly string dateFormat = Constants.DateTimeFormat.DdMMYyyy;
        private readonly string timeFormat = Constants.DateTimeFormat.HHmmss;
        private readonly string dateTimeFormat = string.Empty;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="dateFormat"> format data to parse date value </param>
        /// <param name="timeFormat"> format data to parse time value </param>
        public DateTimeFilterModel(string dateFormat = "", string timeFormat = "") 
        {
            if (!string.IsNullOrWhiteSpace(dateFormat))
            {
                this.dateFormat = dateFormat;
            }

            if (!string.IsNullOrWhiteSpace(timeFormat))
            {
                this.timeFormat = timeFormat;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="dateFormat"> format data to parse date value </param>
        /// <param name="timeFormat"> format data to parse time value </param>
        public DateTimeFilterModel(string dateTimeFormat)
        {
            if (!string.IsNullOrWhiteSpace(dateTimeFormat))
            {
                this.dateTimeFormat = dateTimeFormat;
            }
        }

        internal string GetDateTimeFormat()
        {
            if (!string.IsNullOrWhiteSpace(dateTimeFormat))
                return dateTimeFormat;

            return $"{dateFormat} {timeFormat}";
        }

        public DateTime GetDateTime_From()
        {
            string dateTimeString = DateFrom;

            if (string.IsNullOrWhiteSpace(TimeFrom))
                dateTimeString += " 00:00:00";
            else
                dateTimeString += $" {TimeFrom}";

            if (DateTime.TryParseExact(dateTimeString, $"{GetDateTimeFormat()}", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;
            else
                return new DateTime();
        }

        public DateTime GetDateTime_To()
        {
            string dateTimeString = DateTo;

            if (string.IsNullOrWhiteSpace(TimeTo))
                dateTimeString += " 23:59:59";
            else
                dateTimeString += $" {TimeTo}";

            if (DateTime.TryParseExact(dateTimeString, $"{GetDateTimeFormat()}", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;
            else
                return new DateTime();
        }

        /// <summary>
        /// The start date on which the searching is based
        /// </summary>
        public string DateFrom { get; set; }

        /// <summary>
        /// The end date on which the searching is based
        /// </summary>
        public string DateTo { get; set; }

        /// <summary>
        /// The start time on which the searching is based
        /// </summary>
        public string TimeFrom { get; set; }

        /// <summary>
        /// The end time on which the searching is based
        /// </summary>
        public string TimeTo { get; set; }
    }
}
