using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Card;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Service.Protocol;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Config mapping for Card
    /// </summary>
    public class CardMapping : Profile
    {
        /// <summary>
        /// ctor card mapping
        /// </summary>
        public CardMapping()
        {

            CreateMap<CardModel, Card>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Description));

            CreateMap<Card, CardModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedOn.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)));

            CreateMap<Face, FaceModel>()
                .ForMember(dest => dest.LeftIrisImage, opt => opt.MapFrom(src => src.LeftIrisImage))
                .ForMember(dest => dest.RightIrisImage, opt => opt.MapFrom(src => src.RightIrisImage))
                .ForMember(dest => dest.FaceImage, opt => opt.MapFrom(src => src.FaceImage))
                .ForMember(dest => dest.FaceSmallImage, opt => opt.MapFrom(src => src.FaceSmallImage))
                .ForMember(dest => dest.LeftIrisCode, opt => opt.MapFrom(src => src.LeftIrisCode))
                .ForMember(dest => dest.RightIrisCode, opt => opt.MapFrom(src => src.RightIrisCode))
                .ForMember(dest => dest.FaceCode, opt => opt.MapFrom(src => src.FaceCode));

            CreateMap<FaceModel, Face>()
                .ForMember(dest => dest.LeftIrisImage, opt => opt.MapFrom(src => src.LeftIrisImage))
                .ForMember(dest => dest.RightIrisImage, opt => opt.MapFrom(src => src.RightIrisImage))
                .ForMember(dest => dest.FaceImage, opt => opt.MapFrom(src => src.FaceImage))
                .ForMember(dest => dest.FaceSmallImage, opt => opt.MapFrom(src => src.FaceSmallImage))
                .ForMember(dest => dest.LeftIrisCode, opt => opt.MapFrom(src => src.LeftIrisCode))
                .ForMember(dest => dest.RightIrisCode, opt => opt.MapFrom(src => src.RightIrisCode))
                .ForMember(dest => dest.FaceCode, opt => opt.MapFrom(src => src.FaceCode));

            CreateMap<FingerPrint, FingerPrintModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.Templates, opt => opt.MapFrom(src => JsonConvert.DeserializeObject<List<string>>(!string.IsNullOrWhiteSpace(src.Templates) ? src.Templates : "[]")));
            
            CreateMap<FingerPrintModel, FingerPrint>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.Templates, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Templates ?? new List<string>())));

            CreateMap<Card, CardInfoBasic>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EffectiveDateUtc, opt => opt.MapFrom(src => src.User != null ? src.User.EffectiveDate : src.Visit != null ? src.Visit.StartDate : DateTime.MinValue))
                .ForMember(dest => dest.ExpireDateUtc, opt => opt.MapFrom(src => src.User != null ? src.User.ExpiredDate : src.Visit != null ? src.Visit.EndDate : DateTime.MinValue))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.User != null && src.User.EffectiveDate.HasValue ? src.User.EffectiveDate.Value.ToString(Constants.DateTimeFormatDefault) : src.Visit != null ? src.Visit.StartDate.ToString(Constants.DateTimeFormatDefault) : string.Empty))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.User != null && src.User.ExpiredDate.HasValue ? src.User.ExpiredDate.Value.ToString(Constants.DateTimeFormatDefault) : src.Visit != null ? src.Visit.EndDate.ToString(Constants.DateTimeFormatDefault) : string.Empty))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.User != null ? src.User.EmpNumber : src.Visit != null ? src.Visit.VisitorEmpNumber : String.Empty))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : src.Visit != null ? src.Visit.VisitorName : String.Empty))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.User != null ? src.User.Department.DepartName : src.Visit != null ? src.Visit.VisitorDepartment : String.Empty))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                // .ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                // .ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.Id))
                // .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.User != null ? src.User.KeyPadPw : string.Empty))
                .ForMember(dest => dest.IsMasterCard, opt => opt.MapFrom(src => src.User != null && src.User.IsMasterCard))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.User != null ? src.User.AccessGroupId : src.Visit != null ? src.Visit.AccessGroupId : src.AccessGroupId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId ?? (src.VisitId ?? 0)))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.User != null ? src.User.Position : src.Visit != null ? src.Visit.Position : string.Empty))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User != null ? src.User.Avatar : src.Visit != null ? src.Visit.Avatar : string.Empty))
                // .ForMember(dest => dest.FaceData, opt => opt.MapFrom(src => src.Id))
                // .ForMember(dest => dest.FloorIndex, opt => opt.MapFrom(src => src.Id))
                // .ForMember(dest => dest.MasterFlag, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FingerTemplates, opt => opt.MapFrom(src => src.FingerPrint.Select(m => m.Templates)))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Visit, opt => opt.MapFrom(src => src.Visit));

        }
    }
}
