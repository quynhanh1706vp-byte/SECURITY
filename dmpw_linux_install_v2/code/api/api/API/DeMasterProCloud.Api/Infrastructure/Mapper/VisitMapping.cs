using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Vehicle;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.DataModel.VisitArmy;
using DeMasterProCloud.Service.Protocol;
using Namotion.Reflection;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Config mapping for user
    /// </summary>
    public class VisitMapping : Profile
    {
        /// <summary>
        /// ctor user mapping
        /// </summary>
        public VisitMapping()
        {
            CreateMap<VisitModel, Visit>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.VisitType, opt => opt.MapFrom(src => src.VisitType))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.BirthDay) 
                    ? src.BirthDay.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault) 
                    : src.StartDate.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.VisitorDepartment, opt => opt.MapFrom(src => src.VisitorDepartment))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.VisiteeSite, opt => opt.MapFrom(src => src.VisiteeSite))
                .ForMember(dest => dest.VisitPlace, opt => opt.MapFrom(src => src.VisitPlace))
                .ForMember(dest => dest.VisitReason, opt => opt.MapFrom(src => src.VisitReason))
                .ForMember(dest => dest.VisitorName, opt => opt.MapFrom(src => src.VisitorName))
                .ForMember(dest => dest.VisiteeName, opt => opt.MapFrom(src => src.VisiteeName))
                .ForMember(dest => dest.VisiteeDepartment, opt => opt.MapFrom(src => src.VisiteeDepartment))
                .ForMember(dest => dest.VisiteeDepartmentId, opt => opt.MapFrom(src => src.VisiteeDepartmentId))
                .ForMember(dest => dest.VisiteeEmpNumber, opt => opt.MapFrom(src => src.VisiteeEmpNumber))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.InvitePhone, opt => opt.MapFrom(src => src.InvitePhone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.IsDecision, opt => opt.MapFrom(src => src.IsDecision))
                .ForMember(dest => dest.VisitingCardState, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.ApproverId1, opt => opt.MapFrom(src => src.ApproverId))
                .ForMember(dest => dest.ApproverId2, opt => opt.MapFrom(src => src.ApproverId))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.VisitorEmpNumber, opt => opt.MapFrom(src => src.VisitorEmpNumber))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber))
                .ForMember(dest => dest.IdentificationType, opt => opt.MapFrom(src => src.IdentificationType))
                .ForMember(dest => dest.IdentificationTypeName, opt => opt.MapFrom(src => src.IdentificationTypeName))
                .ForMember(dest => dest.LeaderId, opt => opt.MapFrom(src => src.LeaderId))
                .ForMember(dest => dest.LeaderName, opt => opt.MapFrom(src => src.LeaderName))
                .ForMember(dest => dest.AllowedBelonging, opt => opt.MapFrom(src => src.AllowedBelonging))
                //.ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.PlaceIssueIdNumber, opt => opt.MapFrom(src => src.PlaceIssueIdNumber))
                .ForMember(dest => dest.DateIssueIdNumber, opt => opt.MapFrom(src => src.DateIssueIdNumber))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.RoomNumber))
                .ForMember(dest => dest.RoomDoorCode, opt => opt.MapFrom(src => src.RoomDoorCode))
                //.ForMember(dest => dest.AutoApproved, opt => opt.MapFrom(src => src.AutoApproved))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.UnitName))
                .ForMember(dest => dest.DocumentLabel, opt => opt.MapFrom(src => src.DocumentLabel))
                .ForMember(dest => dest.DocumentNumber, opt => opt.MapFrom(src => src.DocumentNumber))
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType))
                .ForMember(dest => dest.NationalIdCard, opt => opt.Ignore());

            CreateMap<Visit, VisitDataModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.VisitType, opt => opt.MapFrom(src => src.VisitType))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.BirthDay.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.VisitorDepartment, opt => opt.MapFrom(src => src.VisitorDepartment))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.VisiteeSite, opt => opt.MapFrom(src => src.VisiteeSite))
                .ForMember(dest => dest.VisitReason, opt => opt.MapFrom(src => src.VisitReason))
                .ForMember(dest => dest.VisitorName, opt => opt.MapFrom(src => src.VisitorName))
                .ForMember(dest => dest.VisiteeName, opt => opt.MapFrom(src => src.VisiteeName))
                .ForMember(dest => dest.VisiteeDepartment, opt => opt.MapFrom(src => src.VisiteeDepartment))
                .ForMember(dest => dest.VisiteeEmpNumber, opt => opt.MapFrom(src => src.VisiteeEmpNumber))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.InvitePhone, opt => opt.MapFrom(src => src.InvitePhone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.IsDecision, opt => opt.MapFrom(src => src.IsDecision))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.VisitingCardState))
                .ForMember(dest => dest.ApproverId, opt => opt.MapFrom(src => src.ApproverId1))
                .ForMember(dest => dest.ApproverId, opt => opt.MapFrom(src => src.ApproverId2))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.ProcessStatus, opt => opt.MapFrom(src => ((VisitChangeStatusType) src.Status).GetDescription()))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.IdentificationType, opt => opt.MapFrom(src => src.IdentificationType))
                .ForMember(dest => dest.IdentificationTypeName, opt => opt.MapFrom(src => src.IdentificationTypeName))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber))
                .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.LeaderId, opt => opt.MapFrom(src => src.LeaderId))
                .ForMember(dest => dest.LeaderName, opt => opt.MapFrom(src => src.LeaderName))
                .ForMember(dest => dest.AllowedBelonging, opt => opt.MapFrom(src => src.AllowedBelonging))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.PlaceIssueIdNumber, opt => opt.MapFrom(src => src.PlaceIssueIdNumber))
                .ForMember(dest => dest.DateIssueIdNumber, opt => opt.MapFrom(src => src.DateIssueIdNumber))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.UnitName))
                // .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartDate))
                // .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.RoomNumber))
                .ForMember(dest => dest.RoomDoorCode, opt => opt.MapFrom(src => src.RoomDoorCode))
                                //.ForMember(dest => dest.AutoApproved, opt => opt.MapFrom(src => src.AutoApproved))
                .ForMember(dest => dest.DocumentLabel, opt => opt.MapFrom(src => src.DocumentLabel))
                .ForMember(dest => dest.DocumentNumber, opt => opt.MapFrom(src => src.DocumentNumber))
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.NationalIdCard, opt => opt.MapFrom(src => src.NationalIdCard));


            CreateMap<Visit, VisitListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ApplyDate,
                    opt => opt.MapFrom(src => src.ApplyDate.HasValue ? src.ApplyDate.Value.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault) 
                        : src.ApplyDate.ToSettingDateTimeString()))
                .ForMember(dest => dest.VisitorName, opt => opt.MapFrom(src => src.VisitorName))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.BirthDay.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.VisitorDepartment, opt => opt.MapFrom(src => src.VisitorDepartment))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.VisiteeSite, opt => opt.MapFrom(src => src.VisiteeSite))
                .ForMember(dest => dest.VisitReason, opt => opt.MapFrom(src => src.VisitReason))
                .ForMember(dest => dest.VisiteeName, opt => opt.MapFrom(src => src.VisiteeName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.InvitePhone, opt => opt.MapFrom(src => src.InvitePhone))
                .ForMember(dest => dest.ProcessStatus,
                    opt => opt.MapFrom(src => ((VisitChangeStatusType)src.Status).GetDescription()))
                .ForMember(dest => dest.Approver1, opt => opt.MapFrom(src => src.ApproverId1.ToString()))
                .ForMember(dest => dest.Approver2, opt => opt.MapFrom(src => src.ApproverId2.ToString()))
                .ForMember(dest => dest.RejectReason, opt => opt.MapFrom(src => src.RejectReason))
                .ForMember(dest => dest.IsDecision, opt => opt.MapFrom(src => src.IsDecision))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.IdentificationType, opt => opt.MapFrom(src => src.IdentificationType))
                .ForMember(dest => dest.IdentificationTypeName, opt => opt.MapFrom(src => src.IdentificationTypeName))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => src.UpdatedBy))
                .ForMember(dest => dest.Doors, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.AccessGroup.AccessGroupDevice.Where(m => !m.Icu.IsDeleted).Select(m => m.Icu.Name))))
                .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.VisitType, opt =>
                {
                    int numType;
                    opt.MapFrom(src => int.TryParse(src.VisitType, out numType) ? numType : 0);
                })
                //.ForMember(dest => dest.VisitPlace, opt => opt.MapFrom(src => src.VisitArmy.Any() ? src.VisitArmy.First().VisitPlace : ""))
                .ForMember(dest => dest.VisitPlace, opt => opt.MapFrom(src => src.VisitPlace))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.RoomNumber))
                .ForMember(dest => dest.RoomDoorCode, opt => opt.MapFrom(src => src.RoomDoorCode))
                .ForMember(dest => dest.AllowedBelonging, opt => opt.MapFrom(src => src.AllowedBelonging))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.DocumentLabel, opt => opt.MapFrom(src => src.DocumentLabel))
                .ForMember(dest => dest.DocumentNumber, opt => opt.MapFrom(src => src.DocumentNumber))
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Gender));

            CreateMap<Visit, SendUserDetail>()
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.StartDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.EndDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.VisitorName));

            CreateMap<SystemLog, VisitListHistoryModel>()
               .ForMember(dest => dest.EventDetails, opt => opt.MapFrom(src => src.Content + "\n" + src.ContentDetails));

            CreateMap<EventLog, VisitListHistoryModel>()
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.DoorName.ToString()))
                .ForMember(dest => dest.EventTime, opt => opt.MapFrom(src => src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                //.ForMember(dest => dest.Antipass, opt => opt.MapFrom(src => Constants.AntiPass.Contains(src.Antipass)
                //    ? ((Antipass)Enum.Parse(typeof(Antipass), src.Antipass)).GetDescription()
                //    : EventLogResource.lblUnknown));
                .ForMember(dest => dest.Antipass, opt => opt.MapFrom(src => Constants.AntiPass.ToList().FindIndex(x => x.Equals(src.Antipass, StringComparison.OrdinalIgnoreCase)) != -1
                    ? ((Antipass)Enum.Parse(typeof(Antipass), src.Antipass, true)).GetDescription()
                    : EventLogResource.lblUnknown));
            
            CreateMap<Card, VisitCardInfo>()
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => src.VisitId))
                .ForMember(dest => dest.ManagementNumber, opt => opt.MapFrom(src => src.ManagementNumber))
                .ForMember(dest => dest.CardRoleType, opt => opt.MapFrom(src => src.CardRoleType))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.AssignedCardDate, opt => opt.MapFrom(src => src.Visit.IssuedDate))
                .ForMember(dest => dest.VisitType, opt => opt.MapFrom(src => src.Visit.VisitType))
                .ForMember(dest => dest.VisitorName, opt => opt.MapFrom(src => src.Visit.VisitorName))
                .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Visit.BirthDay))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Visit.Position))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Visit.Address))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Visit.Phone))
                .ForMember(dest => dest.VisitPlace, opt => opt.MapFrom(src =>
                    src.Visit != null
                        ? src.Visit.VisitPlace
                        : ""
                ))
                .ForMember(dest => dest.VisitPurpose, opt => opt.MapFrom(src => src.Visit.VisitReason))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Visit.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Visit.EndDate))
                .ForMember(dest => dest.VisiteeName, opt => opt.MapFrom(src => src.Visit.VisiteeName))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Visit.VisiteeDepartment))
                .ForMember(dest => dest.VehicleNumber, opt => opt.MapFrom(src =>
                    src.Visit != null && src.Visit.Vehicle.Any(v => !v.IsDeleted)
                        ? src.Visit.Vehicle.First(v => !v.IsDeleted).PlateNumber
                        : null
                ));

            CreateMap<VisitSetting, VisitSettingInitModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
                .ForMember(dest => dest.FirstApproverAccounts, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.FirstApproverAccounts) ? "[]" : src.FirstApproverAccounts))
                .ForMember(dest => dest.SecondsApproverAccounts, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.SecondsApproverAccounts) ? "[]" : src.SecondsApproverAccounts))
                .ForMember(dest => dest.VisitCheckManagerAccounts, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.VisitCheckManagerAccounts) ? "[]" : src.VisitCheckManagerAccounts))
                .ForMember(dest => dest.GroupDevices, opt => opt.MapFrom(src => src.GroupDevices))
                .ForMember(dest => dest.ApprovalStepNumber, opt => opt.MapFrom(src => src.ApprovalStepNumber))
                .ForMember(dest => dest.OutSide, opt => opt.MapFrom(src => src.OutSide))
                .ForMember(dest => dest.AllowEmployeeInvite, opt => opt.MapFrom(src => src.AllowEmployeeInvite))
                .ForMember(dest => dest.EnableCaptCha, opt => opt.MapFrom(src => src.EnableCaptCha))
                .ForMember(dest => dest.EnableAutoApproval, opt => opt.MapFrom(src => src.EnableAutoApproval))
                .ForMember(dest => dest.OnlyAccessSingleBuilding, opt => opt.MapFrom(src => src.OnlyAccessSingleBuilding))
                .ForMember(dest => dest.InsiderAutoApproved, opt => opt.MapFrom(src => src.InsiderAutoApproved))
                .ForMember(dest => dest.AllowDeleteRecord, opt => opt.MapFrom(src => src.AllowDeleteRecord))
                .ForMember(dest => dest.AllowEditRecord, opt => opt.MapFrom(src => src.AllowEditRecord))
                .ForMember(dest => dest.AllLocationWarning, opt => opt.MapFrom(src => src.AllLocationWarning))
                .ForMember(dest => dest.DeviceIdCheckIn, opt => opt.MapFrom(src => src.DeviceIdCheckIn))
                .ForMember(dest => dest.ListFieldsEnable, opt => opt.MapFrom(src => src.ListFieldsEnable))
                .ForMember(dest => dest.AllowGetUserTarget, opt => opt.MapFrom(src => src.AllowGetUserTarget))
                .ForMember(dest => dest.PersonalInvitationLink, opt => opt.MapFrom(src => src.PersonalInvitationLink))
                .ForMember(dest => dest.AllowSelectDoorWhenCreateNew, opt => opt.MapFrom(src => src.AllowSelectDoorWhenCreateNew))
                .ForMember(dest => dest.FieldRegisterLeft, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.FieldRegisterLeft) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(src.FieldRegisterLeft)))
                .ForMember(dest => dest.FieldRegisterRight, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.FieldRegisterRight) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(src.FieldRegisterRight)))
                .ForMember(dest => dest.FieldRequired, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.FieldRequired) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(src.FieldRequired)))
                .ForMember(dest => dest.DefaultDoors, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.DefaultDoors) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(src.DefaultDoors)))
                .ForMember(dest => dest.AllowSendKakao, opt => opt.MapFrom(src => src.AllowSendKakao))
                .ForMember(dest => dest.ListVisitPurpose, opt => opt.MapFrom(src => src.ListVisitPurpose));


            CreateMap<VisitorImportModel, Visit>()
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.BirthDay.ToString())
                        ? src.BirthDay.ToString()
                            .ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)
                        : src.StartDate.Value.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.VisitorDepartment, opt => opt.MapFrom(src => src.VisitorDepartment.Value))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position.Value))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src =>
                        src.StartDate.Value.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EndDate,
                    opt => opt.MapFrom(src =>
                        src.EndDate.Value.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.VisiteeSite, opt => opt.MapFrom(src => src.VisiteeSite.Value))
                .ForMember(dest => dest.VisitReason, opt => opt.MapFrom(src => src.VisitReason.Value))
                .ForMember(dest => dest.VisitorName, opt => opt.MapFrom(src => src.VisitorName.Value))
                .ForMember(dest => dest.VisiteeName, opt => opt.MapFrom(src => src.VisiteeName.Value))
                .ForMember(dest => dest.VisiteeDepartment, opt => opt.MapFrom(src => src.VisiteeDepartment.Value))
                .ForMember(dest => dest.VisiteeDepartmentId, opt => opt.MapFrom(src => src.VisiteeDepartmentId.Value))
                .ForMember(dest => dest.VisiteeEmpNumber, opt => opt.MapFrom(src => src.VisiteeEmpNumber.Value))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Value))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address.Value))
                .ForMember(dest => dest.ApproverId1, opt => opt.MapFrom(src => (int)src.ApproverId1.Value))
                .ForMember(dest => dest.ApproverId2, opt => opt.MapFrom(src => (int)src.ApproverId2.Value))
                .ForMember(dest => dest.CardId, opt => opt.Ignore())
                .ForMember(dest => dest.VisitorEmpNumber, opt => opt.MapFrom(src => src.VisitorEmpNumber.Value))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId.Value))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber.Value))
                .ForMember(dest => dest.VisiteeId, opt => opt.MapFrom(src => src.VisiteeId.Value));
        }
    }
}
