using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.UserLog;
using DeMasterProCloud.Service.Protocol;
using System.Linq;
using DeMasterProCloud.DataModel.DeviceSDK;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Define usser log mapping
    /// </summary>
    public class UserLogMapping : Profile
    {
        /// <summary>
        /// Define usser log mapping ( UserLog to UserProtocolDetailData )
        /// </summary>
        public UserLogMapping()
        {
            CreateMap<UserLog, SDKCardModel>()
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.User.EmpNumber))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? (src.User.FirstName + " " + src.User.LastName).Trim() : src.Visit.VisitorName))
                // IsMasterCard
                //.ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.User.BuildingMaster.Count == 0 ? (src.User.IsMasterCard ? 1 : 0) : 1))
                .ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.MasterFlag))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                //.ForMember(dest => dest.DepartmentName,
                //    opt => opt.MapFrom(src => src.User.Department.DepartName))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartmentName))
                .ForMember(dest => dest.WorkType, opt => opt.MapFrom(src => src.WorkType))
                .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => src.Grade))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.EffectiveDate,
                    opt => opt.MapFrom(src =>
                        src.EffectiveDate.HasValue
                            ? src.EffectiveDate.Value.ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                            : null))
                // .ForMember(dest => dest.EffectiveDateTime,
                //     opt => opt.MapFrom(src =>
                //         src.EffectiveDate.HasValue
                //             ? src.EffectiveDate.Value.ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                //             : null))
                .ForMember(dest => dest.ExpireDate,
                    opt => opt.MapFrom(src =>
                        src.ExpiredDate.HasValue
                            ? src.ExpiredDate.Value.ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                            : null))
                // .ForMember(dest => dest.ExpireDateTime,
                //     opt => opt.MapFrom(src =>
                //         src.ExpiredDate.HasValue
                //             ? src.ExpiredDate.Value.ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                //             : null))
                .ForMember(dest => dest.Password,
                    opt => opt.MapFrom(src =>
                        !string.IsNullOrWhiteSpace(src.User.KeyPadPw)
                            ? Encryptor.Decrypt(src.User.KeyPadPw,
                                ApplicationVariables.Configuration[Constants.Settings.EncryptKey])
                            : ""))
                //.ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.User.CardStatus))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.TzPosition))
                .ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.AntiPassBack))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.User.AccessGroupId))
                .ForMember(dest => dest.FingerTemplates, opt => opt.MapFrom(src => src.FingerTemplates));


            CreateMap<Card, UserLog>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Visit, opt => opt.MapFrom(src => src.Visit))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User != null ? src.User.Id : 0))
                .ForMember(dest => dest.KeyPadPw, opt => opt.MapFrom(src => src.User != null ? src.User.KeyPadPw : ""))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.User != null ? src.User.EffectiveDate : (src.Visit != null ? src.Visit.StartDate : new System.DateTime())))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.User != null ? src.User.ExpiredDate : (src.Visit != null ? src.Visit.EndDate : new System.DateTime())))
                .ForMember(dest => dest.MasterFlag, opt => opt.MapFrom(src => src.User != null ? src.User.IsMasterCard : false))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User != null ? src.User.Avatar : (src.Visit != null ? src.Visit.Avatar : "")))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.User != null ? src.User.Department.DepartName : (src.Visit != null ? src.Visit.VisitorDepartment : "")))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.User != null ? src.User.Position : (src.Visit != null ? src.Visit.VisitReason : "")))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.FingerTemplates, opt => opt.MapFrom(src => src.FingerPrint != null && src.FingerPrint.Count != 0 ? src.FingerPrint.Select(m => m.Templates) : null));
            //    .Include<Card, UserAvatarLog>();
            //CreateMap<Card, UserAvatarLog>();

            CreateMap<UserProtocolData, UserProtocolDataIcu>()
                .ForMember(dest => dest.MsgId, opt => opt.MapFrom(src => src.MsgId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender))
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

            CreateMap<UserProtocolHeaderData, UserProtocolIcuHeaderData>()
                .ForMember(dest => dest.FrameIndex, opt => opt.MapFrom(src => src.FrameIndex))
                .ForMember(dest => dest.TotalIndex, opt => opt.MapFrom(src => src.TotalIndex))
                .ForMember(dest => dest.UpdateFlag, opt => opt.MapFrom(src => src.UpdateFlag))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users));

            CreateMap<UserProtocolDetailData, UserProtocolDetailDataIcu>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmployeeNumber))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.AdminFlag))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
                .ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.AntiPassBack))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.IdType));

            
            CreateMap<UserProtocolData, UserProtocolDataIcuElevator>()
                .ForMember(dest => dest.MsgId, opt => opt.MapFrom(src => src.MsgId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender))
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

            CreateMap<UserProtocolHeaderData, UserProtocolIcuElevatorHeaderData>()
                .ForMember(dest => dest.FrameIndex, opt => opt.MapFrom(src => src.FrameIndex))
                .ForMember(dest => dest.TotalIndex, opt => opt.MapFrom(src => src.TotalIndex))
                .ForMember(dest => dest.UpdateFlag, opt => opt.MapFrom(src => src.UpdateFlag))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users));
            
            CreateMap<UserProtocolDetailData, UserProtocolDetailDataIcuElevator>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmployeeNumber))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.AdminFlag))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
                .ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.AntiPassBack))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.IdType));
            
            CreateMap<CardInfoBasic, UserProtocolDetailData>()
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmployeeNumber))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartmentName))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.AdminFlag))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.AntiPassBack))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.IdType))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.FaceData, opt => opt.MapFrom(src => src.FaceData))
                .ForMember(dest => dest.FloorIndex, opt => opt.MapFrom(src => src.FloorIndex))
                .ForMember(dest => dest.FingerTemplates, opt => opt.MapFrom(src => src.FingerTemplates));
            
            CreateMap<CardInfoBasic, UserProtocolDetailDataIcu>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmployeeNumber))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.AdminFlag))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
                .ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.AntiPassBack))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.IdType));
            
            CreateMap<CardInfoBasic, UserProtocolDetailDataIcuElevator>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmployeeNumber))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.AdminFlag))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
                .ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.AntiPassBack))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.IdType));
        }
    }
}
