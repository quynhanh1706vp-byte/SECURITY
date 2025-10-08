using System.Text.RegularExpressions;
using FluentValidation;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.SystemLog;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Service;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Report validation
    /// </summary>
    public class ReportValidation : AbstractValidator<EventLogViewModel>
    {
        /// <summary>
        /// Report validation
        /// </summary>
        public ReportValidation()
        {
            RuleFor(m => m.AccessTimeTo).Matches(@"^((([1-9])|(1[0-2])):([0-5])([0-9])\s(A|P)M)$",
                RegexOptions.IgnoreCase).WithMessage(string.Format(MessageResource.InvalidTimeFormat, EventLogResource.lblAccessTime));
            RuleFor(m => m.AccessTimeFrom).Matches(@"^((([1-9])|(1[0-2])):([0-5])([0-9])\s(A|P)M)$",
                RegexOptions.IgnoreCase).WithMessage(string.Format(MessageResource.InvalidTimeFormat, EventLogResource.lblAccessTime));
        }
    }

    /// <summary>
    /// SystemLog Validation
    /// </summary>
    public class SystemLogValidation : AbstractValidator<SystemLogModel>
    {
        /// <summary>
        /// SystemLog Validation
        /// </summary>
        public SystemLogValidation()
        {
            RuleFor(m => m.OpeTimeTo).Matches(@"^((([1-9])|(1[0-2])):([0-5])([0-9])\s(A|P)M)$",
                RegexOptions.IgnoreCase).WithMessage(string.Format(MessageResource.InvalidTimeFormat, SystemLogResource.lblOperationTime));
            RuleFor(m => m.OpeTimeFrom).Matches(@"^((([1-9])|(1[0-2])):([0-5])([0-9])\s(A|P)M)$",
                RegexOptions.IgnoreCase).WithMessage(string.Format(MessageResource.InvalidTimeFormat, SystemLogResource.lblOperationTime));
            RuleFor(m => m.OpeDateFrom)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.OpeDateFrom))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, SystemLogResource.lblOpeDateFrom,
                    Helpers.GetDateServerFormat()));
            RuleFor(m => m.OpeDateTo)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.OpeDateTo))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, SystemLogResource.lblOpeDateTo,
                    Helpers.GetDateServerFormat()));

        }
    }

    /// <summary>
    /// SystemLog OperationTime Validation
    /// </summary>
    public class SystemLogOperationTimeValidation : AbstractValidator<SystemLogOperationTime>
    {
        /// <summary>
        /// SystemLog OperationTime Validation
        /// </summary>
        public SystemLogOperationTimeValidation()
        {
            RuleFor(m => m.OpeDateFrom)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.OpeDateFrom))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, SystemLogResource.lblOpeDateFrom,
                    Helpers.GetDateServerFormat()));

            RuleFor(m => m.OpeDateTo)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.OpeDateTo))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, SystemLogResource.lblOpeDateTo,
                    Helpers.GetDateServerFormat()));

            RuleFor(m => m.OpeTimeFrom)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.OpeTimeFrom))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, SystemLogResource.lblOpeTimeFrom,
                    Helpers.GetDateServerFormat()));

            RuleFor(m => m.OpeTimeTo)
                .Must((m, c) => DateTimeHelper.IsDateTime(m.OpeTimeTo))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, SystemLogResource.lblOpeTimeTo,
                    Helpers.GetDateServerFormat()));
        }
    }
}
