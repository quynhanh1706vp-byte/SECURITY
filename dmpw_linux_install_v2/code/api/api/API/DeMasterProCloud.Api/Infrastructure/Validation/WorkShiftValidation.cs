using FluentValidation;
using DeMasterProCloud.DataModel.WorkShift;
using System.Text.RegularExpressions;
using DeMasterProCloud.Common.Resources;
using System;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// WorkShift validation
    /// </summary>
    public class WorkShiftValidation : BaseValidation<WorkShiftModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// WorkShift validation
        /// </summary>
        public WorkShiftValidation(IWorkShiftService workShiftService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Name)
            .NotEmpty()
            .WithMessage(string.Format(MessageResource.MessageCanNotBlank, WorkShiftResource.lblWorkShift))
            .Must((reg, n) => !workShiftService.IsNameExist(CreateModelWithRouteId(reg)))
            .WithMessage(WorkShiftResource.NameAlreadyExists)
            .MaximumLength(100)
            .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, WorkShiftResource.lblWorkShift, "100"))
            .Must(NotContainsCode)
            .WithMessage(string.Format(MessageResource.msgInvalidInput, WorkShiftResource.lblWorkShift));
    

            RuleFor(reg => reg.StartTime)
                .NotEmpty()
                .Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
                .WithMessage(WorkShiftResource.TimeIsInvalid);

            RuleFor(reg => reg.EndTime)
                .NotEmpty()
                .Matches(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")
                .WithMessage(WorkShiftResource.TimeIsInvalid);

            RuleFor(reg => reg.EndTime)
                .Must((model, endTime) => TimeSpan.Parse(endTime) > TimeSpan.Parse(model.StartTime))
                .When(model => !string.IsNullOrEmpty(model.StartTime) && !string.IsNullOrEmpty(model.EndTime))
                .WithMessage(WorkShiftResource.StartTimeMustBeLessThanEndTime);
        }

        private WorkShiftModel CreateModelWithRouteId(WorkShiftModel originalModel)
        {
            var modelWithId = new WorkShiftModel
            {
                Id = GetWorkShiftIdFromRoute(),
                Name = originalModel.Name?.Trim(),
                StartTime = originalModel.StartTime?.Trim(),
                EndTime = originalModel.EndTime?.Trim(),
            };
            return modelWithId;
        }

        private int GetWorkShiftIdFromRoute()
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
}
