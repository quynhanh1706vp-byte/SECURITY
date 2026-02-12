using FluentValidation;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Holiday;
using DeMasterProCloud.Service;
using DeMasterProCloud.Common.Infrastructure;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Holiday validation
    /// </summary>
    public class HolidayValidation : BaseValidation<HolidayModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Holiday validation
        /// </summary>
        /// <param name="holidayService"></param>
        /// <param name="httpContextAccessor"></param>
        public HolidayValidation(IHolidayService holidayService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Name).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, HolidayResource.lblHolidayName))
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, HolidayResource.lblHolidayName, "50"))
                .Must((reg, strings) => !holidayService.IsExistedName(GetHolidayIdFromRoute(), reg.Name?.Trim()))
                .WithMessage(reg => string.Format(MessageResource.Exist, reg.Name))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, HolidayResource.lblHolidayName));
            RuleFor(reg => reg.StartDate).NotEmpty()
                .Must((reg, c) => DateTimeHelper.IsDateTime(reg.StartDate))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, HolidayResource.lblStartDate,
                    Helpers.GetDateServerFormat()))
                .Must((reg, strings) => !holidayService.IsOverLapDurationTime(GetHolidayIdFromRoute(), reg.StartDate?.Trim(), reg.EndDate?.Trim()))
                .WithMessage(HolidayResource.msgOverlapDurationTime);
            RuleFor(reg => reg.EndDate).NotEmpty()
                .Must((reg, c) => DateTimeHelper.IsDateTime(reg.StartDate))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, HolidayResource.lblEndDate,
                    Helpers.GetDateServerFormat()))
                .Must((reg, strings) => Helpers.CompareDate(reg.StartDate, reg.EndDate))
                .WithMessage(reg => string.Format(MessageResource.GreaterThan, HolidayResource.lblEndDate, reg.StartDate));
            RuleFor(reg => reg.Type).Must((reg, c) => reg.Type > 0)
                .WithMessage(string.Format(MessageResource.NotSelected, HolidayResource.lblHolidayType));
            RuleFor(reg => reg.Remarks)
                .MaximumLength(1000)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, HolidayResource.lblRemark, "1000"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, HolidayResource.lblRemark));
        }

        private int GetHolidayIdFromRoute()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.RouteValues.TryGetValue("id", out var idValue) == true)
            {
                if (int.TryParse(idValue?.ToString(), out var id))
                {
                    return id;
                }
            }
            return 0; // Default for Add operations
        }
    }

    /// <summary>
    /// A list of Holiday validation
    /// </summary>
    public class HolidaysValidation : AbstractValidator<List<HolidayModel>>
    {
        public HolidaysValidation(IHolidayService holidayService, IHttpContextAccessor httpContextAccessor)
        {
            RuleForEach(x => x).SetValidator(new HolidayValidation(holidayService, httpContextAccessor));
        }
    }
}
