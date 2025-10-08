using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.EventLog;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// EventLog validation
    /// </summary>
    public class EventLogValidation : AbstractValidator<EventLogAccessTimeModel>
    {
        /// <summary>
        /// EventLog validation
        /// </summary>
        public EventLogValidation() 
        {
            RuleFor(m => m.AccessDateFrom)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.AccessDateFrom))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, EventLogResource.lblAccessDateFrom,
                    Helpers.GetDateServerFormat()));

            RuleFor(m => m.AccessDateTo)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.AccessDateTo))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, EventLogResource.lblAccessDateTo,
                    Helpers.GetDateServerFormat()));

            RuleFor(m => m.AccessTimeFrom)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.AccessTimeFrom))
                .WithMessage(string.Format(MessageResource.InvalidTimeFormat, EventLogResource.lblAccessTimeFrom,
                    Helpers.GetDateServerFormat()));

            RuleFor(m => m.AccessTimeTo)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.AccessTimeTo))
                .WithMessage(string.Format(MessageResource.InvalidTimeFormat, EventLogResource.lblAccessTimeTo,
                    Helpers.GetDateServerFormat()));
        }
    }

    //public class EventCountValidation : AbstractValidator<EventCountIdProcessIdModel>
    //{
    //    public EventCountValidation()
    //    {
    //        RuleFor(m => m.).Matches(@"^((([1-9])|(1[0-2])):([0-5])([0-9])\s(A|P)M)$",
    //            RegexOptions.IgnoreCase).WithMessage(string.Format(MessageResource.InvalidTimeFormat, SystemLogResource.lblOperationTime));
    //        RuleFor(m => m.OpeTimeFrom).Matches(@"^((([1-9])|(1[0-2])):([0-5])([0-9])\s(A|P)M)$",
    //            RegexOptions.IgnoreCase).WithMessage(string.Format(MessageResource.InvalidTimeFormat, SystemLogResource.lblOperationTime));
    //        RuleFor(m => m.OpeDateFrom)
    //            .Must((m, c) => DateTimeHelper.IsDateTime(m.OpeDateFrom))
    //            .WithMessage(string.Format(MessageResource.InvalidDateFormat, SystemLogResource.lblOpeDateFrom,
    //                Helpers.GetDateServerFormat()));
    //        RuleFor(m => m.OpeDateTo)
    //            .Must((m, c) => DateTimeHelper.IsDateTime(m.OpeDateTo))
    //            .WithMessage(string.Format(MessageResource.InvalidDateFormat, SystemLogResource.lblOpeDateTo,
    //                Helpers.GetDateServerFormat()));

    //    }
    //}
}
