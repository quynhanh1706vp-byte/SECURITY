using FluentValidation;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Department;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Department validation
    /// </summary>
    public class DepartmentValidation : BaseValidation<DepartmentModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Department validation
        /// </summary>
        /// <param name="departmentService"></param>
        /// <param name="httpContextAccessor"></param>
        public DepartmentValidation(IDepartmentService departmentService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Name).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, DepartmentResource.lblDepartmentName))
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, DepartmentResource.lblDepartmentName, "50"))
                .Must((reg, n) => !departmentService.IsDepartmentNameExist(CreateModelWithRouteId(reg)))
                .WithMessage(string.Format(MessageResource.Exist, DepartmentResource.lblDepartmentName))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, DepartmentResource.lblDepartmentName));

            RuleFor(reg => reg.Number)
                .MaximumLength(20)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, DepartmentResource.lblDepartmentNumber, "20"))
                .Must((reg, n) => string.IsNullOrWhiteSpace(reg.Number) || !departmentService.IsDepartmentNumberExist(CreateModelWithRouteId(reg)))
                .WithMessage(string.Format(MessageResource.Exist, DepartmentResource.lblDepartmentNumber))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, DepartmentResource.lblDepartmentNumber));
            
            //RuleFor(reg => reg.Number).NotEmpty()
            //    .MaximumLength(20);

            //RuleFor(reg => reg.Name).NotEmpty()
            //    .MaximumLength(30);
        }

        private DepartmentModel CreateModelWithRouteId(DepartmentModel originalModel)
        {
            var modelWithId = new DepartmentModel
            {
                Id = GetDepartmentIdFromRoute(),
                Name = originalModel.Name?.Trim(),
                Number = originalModel.Number?.Trim(),
                ParentId = originalModel.ParentId,
                AccessGroupId = originalModel.AccessGroupId,
                DepartmentManagerId = originalModel.DepartmentManagerId,
                MaxNumberCheckout = originalModel.MaxNumberCheckout,
                MaxPercentCheckout = originalModel.MaxPercentCheckout,

            };
            return modelWithId;
        }

        private int GetDepartmentIdFromRoute()
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
