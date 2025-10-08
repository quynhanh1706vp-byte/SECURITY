using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Department;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Config mapping for deparment models
    /// </summary>
    public class DepartmentMapping : Profile
    {
        /// <summary>
        /// ctor no params
        /// </summary>
        public DepartmentMapping()
        {
            CreateMap<Department, DepartmentListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DepartmentNumber, opt => opt.MapFrom(src => src.DepartNo))
                .ForMember(dest => dest.MaxNumberCheckout, opt => opt.MapFrom(src => src.MaxNumberCheckout))
                .ForMember(dest => dest.MaxPercentCheckout, opt => opt.MapFrom(src => src.MaxPercentCheckout))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartName))
                .ForMember(dest => dest.DepartmentManagerId, opt => opt.MapFrom(src => src.DepartmentManagerId == null ? null : src.DepartmentManagerId.ToString()))
                .ForMember(dest => dest.DepartmentManager, opt => opt.MapFrom(src => src.DepartmentManager.Username))
                .ForMember(dest => dest.ParentDepartment, opt => opt.MapFrom(src => src.Parent.DepartName))
                .ForMember(dest => dest.IsRoot, opt => opt.MapFrom(src => src.ParentId == null));

            CreateMap<DepartmentModel, Department>()
                .ForMember(dest => dest.DepartName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DepartNo, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.MaxNumberCheckout, opt => opt.MapFrom(src => src.MaxNumberCheckout))
                .ForMember(dest => dest.MaxPercentCheckout, opt => opt.MapFrom(src => src.MaxPercentCheckout))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId));

            CreateMap<Department, DepartmentModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DepartName))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.DepartNo))
                .ForMember(dest => dest.MaxNumberCheckout, opt => opt.MapFrom(src => src.MaxNumberCheckout))
                .ForMember(dest => dest.MaxPercentCheckout, opt => opt.MapFrom(src => src.MaxPercentCheckout))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
                //.ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.DepartmentAccessGroup != null ? src.DepartmentAccessGroup.AccessGroupId : 0))
                .Include<Department, DepartmentDataModel>();
            CreateMap<Department, DepartmentDataModel>();

            CreateMap<Department, DepartmentModelForUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DepartName));
        }
    }
}
