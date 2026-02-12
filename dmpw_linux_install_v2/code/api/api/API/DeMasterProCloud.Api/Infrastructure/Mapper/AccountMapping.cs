using System;
using System.Web;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.Login;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Create mapping for account
    /// </summary>
    public class AccountMapping : Profile
    {
        /// <summary>
        /// Ctor for mapping account
        /// </summary>
        public AccountMapping()
        {
            CreateMap<Account, LoginModel>()
                .ForMember(dest => dest.Username, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore());
            //.ForMember(dest => dest.CompanyCode, opt => opt.MapFrom(src => src.Company.Code));

            CreateMap<Account, AccountListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => HttpUtility.HtmlEncode(src.Username)))
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src => src.Company.Name));

            //.ForMember(dest => dest.Remarks,
            //    opt => opt.MapFrom(src => src.Remarks))
            //.ForMember(dest => dest.Role,
            //    opt => opt.MapFrom(src =>
            //        HttpUtility.HtmlEncode(((AccountType)src.Type).GetDescription())));
            //.ForMember(dest => dest.Status,
            //    opt => opt.MapFrom(src =>
            //        HttpUtility.HtmlEncode(((Status)src.Status).GetDescription())));

            CreateMap<CompanyAccount, AccountListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => HttpUtility.HtmlEncode(src.Account.Username)))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.DynamicRole.Name))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name));

            CreateMap<AccountModel, Account>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Username, opt =>
                {
                    //opt.Condition(m => m.Id == 0);
                    opt.MapFrom(src => src.Username.ToLower());
                })
                .ForMember(dest => dest.RootFlag, opt => opt.Ignore())
                .ForMember(dest => dest.Password,
                    opt =>
                    {
                        opt.Condition(m => !string.IsNullOrWhiteSpace(m.Password));
                        opt.MapFrom(src => SecurePasswordHasher.Hash(src.Password));
                    })
                .ForMember(dest => dest.CompanyId,
                opt =>
                {
                    // CompanyId = 0 -> not assigned.
                    // [Edward] 2020.01.23
                    // This comment is intended to allow to unassign an account from the company.
                    //opt.Condition(m => m.CompanyId != 0);
                    // If CompanyId is 0, save companyId as null in DB.
                    opt.MapFrom(src => src.CompanyId == 0 ? null : src.CompanyId);
                })
                .ForMember(dest => dest.Language, opt => opt.Ignore())
                .ForMember(dest => dest.TimeZone, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatePasswordOn,
                opt =>
                {
                    opt.Condition(m => !string.IsNullOrWhiteSpace(m.Password) && !string.IsNullOrWhiteSpace(m.ConfirmPassword));
                    opt.MapFrom(src => DateTime.UtcNow);
                })
                .ForMember(dest => dest.PreferredSystem, opt => opt.Ignore());

            CreateMap<Account, AccountModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId,
                opt =>
                {
                    opt.Condition(m => m.CompanyId != null);
                    opt.MapFrom(src => src.Company.Id);
                })
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ForMember(dest => dest.PreferredSystem, opt => opt.MapFrom(src => src.PreferredSystem))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language));
            

            CreateMap<Account, AccountTimeZoneModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                //.ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks))
                /*.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))*/;


            CreateMap<Account, AccountDataModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Company.Id))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.DynamicRoleId));




            CreateMap<CompanyAccount, AccountDataModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.DynamicRoleId));
        }
    }
}
