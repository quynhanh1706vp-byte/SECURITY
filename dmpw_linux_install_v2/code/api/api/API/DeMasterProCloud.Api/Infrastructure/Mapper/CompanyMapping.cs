using System.Linq;
using AutoMapper;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Company;
using DeMasterProCloud.Common.Infrastructure;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// CompanyMapping class
    /// </summary>
    public class CompanyMapping : Profile
    {
        /// <summary>
        /// Company Mapping
        /// </summary>
        public CompanyMapping()
        {
            CreateMap<CompanyModel, Company>()
                .ForMember(dest => dest.Account, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Holiday, opt => opt.Ignore())
                .ForMember(dest => dest.IcuDevice, opt => opt.Ignore())
                .ForMember(dest => dest.SystemLog, opt => opt.Ignore())
                .ForMember(dest => dest.AccessTime, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.Logo, opt =>
                {
                    opt.Condition(m => !string.IsNullOrWhiteSpace(m.Logo));
                    opt.MapFrom(src => System.Text.Encoding.UTF8.GetBytes(src.Logo));
                })
                .ForMember(dest => dest.MiniLogo, opt =>
                {
                    opt.Condition(m => !string.IsNullOrWhiteSpace(m.MiniLogo));
                    opt.MapFrom(src => System.Text.Encoding.UTF8.GetBytes(src.MiniLogo));
                })
                .ForMember(dest => dest.CardBit, opt => opt.MapFrom(src => src.CardBit));
            //.ForMember(dest => dest.ExpiredFrom, opt => opt.MapFrom(src => src.ExpiredFrom))
            //.ForMember(dest => dest.ExpiredTo, opt => opt.MapFrom(src => Helpers.ToEndDateTime(src.ExpiredTo)));

            CreateMap<Company, CompanyModel>()
                .ForMember(dest => dest.Logo, opt =>
                {
                    opt.Condition(src => src.Logo != null);
                    opt.MapFrom(src => System.Text.Encoding.UTF8.GetString(src.Logo));
                })
                .ForMember(dest => dest.MiniLogo, opt =>
                {
                    opt.Condition(src => src.MiniLogo != null);
                    opt.MapFrom(src => System.Text.Encoding.UTF8.GetString(src.MiniLogo));
                })
                .ForMember(dest => dest.CardBit, opt => opt.MapFrom(src => src.CardBit));

            CreateMap<Company, CompanyModelOptions>()
                .ForMember(dest => dest.Logo, opt =>
                {
                    opt.Condition(src => src.Logo != null);
                    opt.MapFrom(src => System.Text.Encoding.UTF8.GetString(src.Logo));
                })
                .ForMember(dest => dest.MiniLogo, opt =>
                {
                    opt.Condition(src => src.MiniLogo != null);
                    opt.MapFrom(src => System.Text.Encoding.UTF8.GetString(src.MiniLogo));
                })
                .ForMember(dest => dest.CardBit, opt => opt.MapFrom(src => src.CardBit));


            CreateMap<Company, CompanyListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
            //    //.ForMember(dest => dest.Admin,
            //    //    opt => opt.MapFrom(src =>
            //    //        src.Account.FirstOrDefault(c => c.RootFlag) != null
            //    //            ? src.Account.FirstOrDefault(c => c.RootFlag).Username
            //    //            : null))
            //    //.ForMember(dest => dest.ExpiredDate,
            //    //    opt => opt.MapFrom(src => src.ExpiredTo.ToSettingDateString()))
            //    .ForMember(dest => dest.CreatedDate,
            //        opt => opt.MapFrom(src => src.CreatedOn.ToSettingDateString()));
        }
    }
}
