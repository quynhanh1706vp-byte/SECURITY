using System;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Card;
using DeMasterProCloud.DataModel.Category;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Service.Protocol;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Config mapping for user
    /// </summary>
    public class UserMapping : Profile
    {
        /// <summary>
        /// ctor user mapping
        /// </summary>
        public UserMapping()
        {
            CreateMap<User, UserListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.UserCode))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Position) ? "" : src.Position))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartName))
                .ForMember(dest => dest.EmployeeNo, opt => opt.MapFrom(src => src.EmpNumber))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.ExpiredDate.ToSettingDateString()))
                .ForMember(dest => dest.CardList, opt => opt.MapFrom(src => src.Card.Where(x => !x.IsDeleted 
                    && x.CardType != (int)CardType.VehicleId && x.CardType != (short)CardType.VehicleMotoBikeId).Select(x => new CardModel() 
                {
                    Id = x.Id,
                    CardId = x.CardId,
                    CardStatus = x.CardStatus,
                    CardType = x.CardType,
                    IssueCount = x.IssueCount,
                    Description = x.Note
                }).ToList()))
                .ForMember(dest => dest.PlateNumberList, opt => opt.MapFrom(src => src.Card
                    .Where(x => !x.IsDeleted && (x.CardType == (int)CardType.VehicleId || x.CardType == (short)CardType.VehicleMotoBikeId)).Select(x => new CardModel()
                {
                    Id = x.Id,
                    CardId = x.CardId,
                    CardStatus = x.CardStatus,
                    CardType = x.CardType,
                    IssueCount = x.IssueCount,
                    Description = x.Note
                }).ToList()))
                .ForMember(dest => dest.FaceList, opt => opt.MapFrom(src => src.Card.Where(x => !x.IsDeleted && (x.CardType == (int)CardType.FaceId
                                                                                                            || x.CardType == (int)CardType.EbknFaceId
                                                                                                            || x.CardType == (int)CardType.BioFaceId
                                                                                                            || x.CardType == (int)CardType.LFaceId
                                                                                                            || x.CardType == (int)CardType.HFaceId)).Select(x => new CardModel()
                {
                    Id = x.Id,
                    CardId = x.CardId,
                    CardStatus = x.CardStatus,
                    CardType = x.CardType,
                    IssueCount = x.IssueCount,
                    Description = x.Note
                }).ToList()))
                .ForMember(dest => dest.AccessGroupName, opt => opt.MapFrom(src => src.AccessGroup.ParentId == null ? src.AccessGroup.Name : src.AccessGroup.Parent.Name + " *"))
                .ForMember(dest => dest.WorkTypeName, opt =>
                {
                    opt.PreCondition(src => src.WorkType != null);
                    opt.MapFrom(src => ((WorkType)src.WorkType).GetDescription());
                })
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber))
                .ForMember(dest => dest.HomePhone, opt => opt.MapFrom(src => src.HomePhone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.ApprovalStatus == (short)ApprovalStatus.NotUse ? ApprovalStatus.Approved.GetDescription() : ((ApprovalStatus)src.ApprovalStatus).GetDescription()))
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId));

            CreateMap<Visit, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.VisitorName))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.UserCode))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.BirthDay))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar));

            CreateMap<UserModel, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PermissionType, opt => opt.MapFrom(src => src.PermissionType))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Trim()))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.KeyPadPw, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Password)
                    ? Encryptor.Encrypt(src.Password, ApplicationVariables.Configuration[Constants.Settings.EncryptKey])
                    : src.Password))
                .ForMember(dest => dest.PassType, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position.Trim()))
                .ForMember(dest => dest.OfficePhone, opt => opt.MapFrom(src => src.OfficePhone))
                .ForMember(dest => dest.PostCode, opt => opt.MapFrom(src => src.PostCode))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.UserCode.Trim()))
                .ForMember(dest => dest.EmpNumber, opt => opt.MapFrom(src => src.EmployeeNumber.Trim()))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.ExpiredDate.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.HomePhone, opt => opt.MapFrom(src => src.HomePhone))
                .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
                .ForMember(dest => dest.IsMasterCard, opt => opt.MapFrom(src => src.IsMasterCard))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.BirthDay.ConvertDefaultStringToDateTime(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.WorkingTypeId, opt => opt.MapFrom(src => src.WorkingTypeId))
                .ForMember(dest => dest.WorkType, opt => opt.MapFrom(src => src.WorkType))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber))
                .ForMember(dest => dest.NationalIdCard, opt => opt.Ignore())
                .Include<UserDataModel, User>();
            CreateMap<UserDataModel, User>();

            CreateMap<User, UserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.PassType))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Trim()))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Sex))
                .ForMember(dest => dest.Password, opt => 
                    opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.KeyPadPw) ? Encryptor.Decrypt(src.KeyPadPw, ApplicationVariables.Configuration[Constants.Settings.EncryptKey]) : String.Empty))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position.Trim()))
                .ForMember(dest => dest.OfficePhone, opt => opt.MapFrom(src => src.OfficePhone))
                .ForMember(dest => dest.PostCode, opt => opt.MapFrom(src => src.PostCode))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.UserCode))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.ExpiredDate.HasValue ? src.ExpiredDate.Value.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault) : null))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate.HasValue ? src.EffectiveDate.Value.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault) : null))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmpNumber.Trim()))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.HomePhone, opt => opt.MapFrom(src => src.HomePhone))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Remarks))
                .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
                .ForMember(dest => dest.IsMasterCard, opt => opt.MapFrom(src => src.IsMasterCard))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.BirthDay.HasValue ? src.BirthDay.Value.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault) : null))
                .ForMember(dest => dest.WorkingTypeId, opt => opt.MapFrom(src => src.WorkingTypeId))
                .ForMember(dest => dest.WorkType, opt => opt.MapFrom(src => src.WorkType))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber))
                .ForMember(dest => dest.NationalIdCard, opt => opt.MapFrom(src => src.NationalIdCard))
                .Include<User, UserDataModel>();
            CreateMap<User, UserDataModel>();


            CreateMap<UserImportExportModel, User>()
                //.ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.UserCode.Value))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Value))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName.Value))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex.Value))
                .ForMember(dest => dest.KeyPadPw, opt =>
                {
                    //opt.Condition(m => !string.IsNullOrEmpty(m.KeyPadPassword.Value));
                    //opt.MapFrom(src => Encryptor.Encrypt(src.KeyPadPassword.Value,
                    //    ApplicationVariables.Configuration[Constants.Settings.EncryptKey]));
                    opt.MapFrom(src => string.Empty);
                })
                //.ForMember(dest => dest.IssuedDate, opt => opt.MapFrom(src => src.IssuedDate.Value))
                .ForMember(dest => dest.IssuedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position.Value))
                .ForMember(dest => dest.OfficePhone, opt => opt.MapFrom(src => src.CompanyPhone.Value))
                .ForMember(dest => dest.PostCode, opt => opt.MapFrom(src => src.PostCode.Value))
                .ForMember(dest => dest.Job, opt => opt.MapFrom(src => src.Job.Value))
                .ForMember(dest => dest.Avatar, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentId, opt => opt.Ignore())
                //.ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId.Value))
                .ForMember(dest => dest.EmpNumber, opt => opt.MapFrom(src => src.EmployeeNumber))
                .ForMember(dest => dest.BirthDay, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiredDate, opt => opt.Ignore())
                .ForMember(dest => dest.EffectiveDate, opt => opt.Ignore())

                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality.Value))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City.Value))
                .ForMember(dest => dest.HomePhone, opt => opt.MapFrom(src => src.HomePhone.Value))
                .ForMember(dest => dest.Responsibility,
                    opt => opt.MapFrom(src => src.Responsibility.Value))
                .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks.Value))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar.Value))
                .ForMember(dest => dest.AccessGroupId, opt =>
                {
                    //opt.MapFrom(src => src.AccessGroupName.Value);

                    opt.MapFrom(src => src.AccessGroupId);
                });
                //.ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount.Value))
                // IsMasterCard
                //.ForMember(dest => dest.IsMasterCard, opt => opt.MapFrom(src => src.IsMasterCard.Value));
            //.ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus.Value));
            CreateMap<User, UserForAccessGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.UserCode))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.ExpiredDate.ToSettingDateString()))
                //.ForMember(dest => dest.AccessGroupName, opt => opt.MapFrom(src => src.AccessGroup.Name))
                .ForMember(dest => dest.AccessGroupName, opt => opt.MapFrom(src => src.AccessGroup.ParentId == null ? src.AccessGroup.Name : src.AccessGroup.Parent.Name + " *"))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => ($"{src.FirstName} {src.LastName}").Trim()))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => ($"{src.FirstName} {src.LastName}").Trim()))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartName))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.WorkTypeName, opt =>
                {
                    opt.PreCondition(src => src.WorkType != null);
                    opt.MapFrom(src => ((WorkType)src.WorkType).GetDescription());
                })
                .ForMember(dest => dest.EmployeeNo, opt => opt.MapFrom(src => src.EmpNumber));

            CreateMap<User, UnAssignUserForAccessGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                //.ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId ?? ""))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartName))
                .ForMember(dest => dest.EmployeeNo, opt => opt.MapFrom(src => src.EmpNumber))
                .ForMember(dest => dest.AccessGroupName, opt => opt.MapFrom(src => src.AccessGroup.ParentId == null ? src.AccessGroup.Name : src.AccessGroup.Parent.Name + " *"));
                //.ForMember(dest => dest.AccessGroupName, opt => opt.MapFrom(src => src.AccessGroup.Name));

            CreateMap<User, AccessibleUserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                //.ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId ?? ""))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department.DepartName))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartName))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmpNumber))
                .ForMember(dest => dest.EmployeeNo, opt => opt.MapFrom(src => src.EmpNumber))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.ExpiredDate.ToSettingDateString()))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.WorkTypeName, opt =>
                {
                    opt.PreCondition(src => src.WorkType != null);
                    opt.MapFrom(src => ((WorkType)src.WorkType).GetDescription());
                })
                .ForMember(dest => dest.EmployeeNo, opt => opt.MapFrom(src => src.EmpNumber))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmpNumber));

            CreateMap<UserDetail, SendUserDetail>()
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate.Substring(0,8)))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate.Substring(0, 8)))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => Helpers.MaskKeyPadPw(src.Password)))
                .ForMember(dest => dest.IsMasterCard, opt => opt.MapFrom(src => src.AdminFlag))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.EmployeeNumber) ? src.EmployeeNumber : string.Empty))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.DepartmentName) ? src.DepartmentName : string.Empty))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.CardStatus,
                    opt => opt.MapFrom(src => ((CardStatus)src.CardStatus).GetDescription()));

            CreateMap<User, SendUserDetail>()
                //.ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.ExpireDate,
                    opt => opt.MapFrom(src => src.ExpiredDate.Value.ToString(Constants.DateTimeFormat.DdMdYyyyFormat)))
                .ForMember(dest => dest.EffectiveDate,
                    opt => opt.MapFrom(src => src.EffectiveDate.Value.ToString(Constants.DateTimeFormat.DdMdYyyyFormat)))
                //.ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.Password,
                    opt => opt.MapFrom(src => Helpers.MaskAndDecrytorKeyPadPw(src.KeyPadPw)))
                // IsMasterCard
                //.ForMember(dest => dest.IsMasterCard, opt => opt.MapFrom(src => src.BuildingMaster.Any() ? 1 : (src.IsMasterCard ? 1 : 0)))
                .ForMember(dest => dest.EmployeeNumber,
                    opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.EmpNumber) ? src.EmpNumber : string.Empty))
                .ForMember(dest => dest.Department,
                    opt => opt.MapFrom(src =>
                        !string.IsNullOrWhiteSpace(src.Department.DepartNo) ? src.Department.DepartName : string.Empty))
                //.ForMember(dest => dest.UserName, opt =>
                //{
                //    opt.Condition(src => !string.IsNullOrEmpty(src.LastName));
                //    opt.MapFrom(src => (src.FirstName + " " + src.LastName).Trim());
                //})
                .ForMember(dest => dest.UserName, opt =>
                {
                    opt.Condition(src => string.IsNullOrWhiteSpace(src.LastName));
                    opt.MapFrom(src => src.FirstName);
                });
            //.ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => ((CardStatus)src.CardStatus).GetDescription()));

            CreateMap<User, UserMasterCardModelDetail>()
                //.ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.Card.))

                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName));

            CreateMap<SendUserDetail, UserMasterCardModelDetail>()
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));
            
            CreateMap<AccessSetting, AccessSettingModel>()
                 .ForMember(dest => dest.AllLocationWarning, opt => opt.MapFrom(src => src.AllLocationWarning))
                 .ForMember(dest => dest.AllowDeleteRecord, opt => opt.MapFrom(src => src.AllowDeleteRecord))
                 .ForMember(dest => dest.ApprovalStepNumber, opt => opt.MapFrom(src => src.ApprovalStepNumber))
                 .ForMember(dest => dest.DeviceIdCheckIn, opt => opt.MapFrom(src => src.DeviceIdCheckIn))
                 .ForMember(dest => dest.EnableAutoApproval, opt => opt.MapFrom(src => src.EnableAutoApproval))
                 .ForMember(dest => dest.FirstApproverAccounts, opt => opt.MapFrom(src => src.FirstApproverAccounts))
                 .ForMember(dest => dest.ListFieldsEnable, opt => opt.Ignore())
                 .ForMember(dest => dest.SecondApproverAccounts, opt => opt.MapFrom(src => src.SecondApproverAccounts));
            
            CreateMap<User, UserListSimpleModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.UserCode))
               .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
               .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
               .ForMember(dest => dest.WorkingTypeId, opt => opt.MapFrom(src => src.WorkingTypeId))
               .ForMember(dest => dest.WorkType, opt => opt.MapFrom(src => src.WorkType))
               .ForMember(dest => dest.EffectiveDate,
                   opt => opt.MapFrom(src => src.EffectiveDate.ToSettingDateString()))
               .ForMember(dest => dest.ExpiredDate,
                   opt => opt.MapFrom(src => src.ExpiredDate.ToSettingDateString()))
               .ForMember(dest => dest.CardList, opt => opt.MapFrom(src => src.Card.Where(x => !x.IsDeleted).Select(x => new CardModel()
               {
                   Id = x.Id,
                   CardId = x.CardId,
                   CardStatus = x.CardStatus,
                   CardType = x.CardType,
                   IssueCount = x.IssueCount,
                   Description = x.Note
               }).ToList()));

            CreateMap<RegisterUserModel, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.BirthDay))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.ExpiredDate))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.WorkType, opt => opt.MapFrom(src => src.WorkType))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.HomePhone, opt => opt.MapFrom(src => src.HomePhone))
                .ForMember(dest => dest.OfficePhone, opt => opt.MapFrom(src => src.OfficePhone))
                .ForMember(dest => dest.PostCode, opt => opt.MapFrom(src => src.PostCode))
                .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar));

            CreateMap<SDKFaceDataList, Face>()
                .ForMember(dest => dest.LeftIrisImage, opt => opt.MapFrom(src => src.LeftIrisImage))
                .ForMember(dest => dest.RightIrisImage, opt => opt.MapFrom(src => src.RightIrisImage))
                .ForMember(dest => dest.FaceImage, opt => opt.MapFrom(src => src.FaceImage))
                .ForMember(dest => dest.FaceSmallImage, opt => opt.MapFrom(src => src.FaceSmallImage))
                .ForMember(dest => dest.LeftIrisCode, opt => opt.MapFrom(src => src.LeftIrisCode))
                .ForMember(dest => dest.RightIrisCode, opt => opt.MapFrom(src => src.RightIrisCode))
                .ForMember(dest => dest.FaceCode, opt => opt.MapFrom(src => src.FaceCode));

            CreateMap<Face, SDKFaceDataList>()
                .ForMember(dest => dest.LeftIrisImage, opt => opt.MapFrom(src => src.LeftIrisImage))
                .ForMember(dest => dest.RightIrisImage, opt => opt.MapFrom(src => src.RightIrisImage))
                .ForMember(dest => dest.FaceImage, opt => opt.MapFrom(src => src.FaceImage))
                .ForMember(dest => dest.FaceSmallImage, opt => opt.MapFrom(src => src.FaceSmallImage))
                .ForMember(dest => dest.LeftIrisCode, opt => opt.MapFrom(src => src.LeftIrisCode))
                .ForMember(dest => dest.RightIrisCode, opt => opt.MapFrom(src => src.RightIrisCode))
                .ForMember(dest => dest.FaceCode, opt => opt.MapFrom(src => src.FaceCode));
            
            CreateMap<NationalIdCardModel, NationalIdCard>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CCCD, opt => opt.MapFrom(src => src.CCCD))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.ConvertDefaultStringToDateTime(Constants.DateTimeFormatDefault)))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.Nation, opt => opt.MapFrom(src => src.Nation))
                .ForMember(dest => dest.Religion, opt => opt.MapFrom(src => src.Religion))
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.District))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.IdentityCharacter, opt => opt.MapFrom(src => src.IdentityCharacter))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate.ConvertDefaultStringToDateTime(Constants.DateTimeFormatDefault)))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.ExpiredDate.ConvertDefaultStringToDateTime(Constants.DateTimeFormatDefault)))
                .ForMember(dest => dest.FatherName, opt => opt.MapFrom(src => src.FatherName))
                .ForMember(dest => dest.MotherName, opt => opt.MapFrom(src => src.MotherName))
                .ForMember(dest => dest.HusbandOrWifeName, opt => opt.MapFrom(src => src.HusbandOrWifeName))
                .ForMember(dest => dest.CMND, opt => opt.MapFrom(src => src.CMND))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar));
            
            CreateMap<NationalIdCard, NationalIdCardModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CCCD, opt => opt.MapFrom(src => src.CCCD))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
                .ForMember(dest => dest.Nation, opt => opt.MapFrom(src => src.Nation))
                .ForMember(dest => dest.Religion, opt => opt.MapFrom(src => src.Religion))
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.District))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.IdentityCharacter, opt => opt.MapFrom(src => src.IdentityCharacter))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)))
                .ForMember(dest => dest.ExpiredDate, opt => opt.MapFrom(src => src.ExpiredDate.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)))
                .ForMember(dest => dest.FatherName, opt => opt.MapFrom(src => src.FatherName))
                .ForMember(dest => dest.MotherName, opt => opt.MapFrom(src => src.MotherName))
                .ForMember(dest => dest.HusbandOrWifeName, opt => opt.MapFrom(src => src.HusbandOrWifeName))
                .ForMember(dest => dest.CMND, opt => opt.MapFrom(src => src.CMND))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar));

        }
    }
}
