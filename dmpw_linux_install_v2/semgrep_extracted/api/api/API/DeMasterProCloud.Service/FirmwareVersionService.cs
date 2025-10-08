using AutoMapper;
using System.Globalization;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.FirmwareVersion;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Protocol;
using DeMasterProCloud.Service.RabbitMqQueue;
using DeMasterProCloud.Common.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Service.Infrastructure;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.DataModel.DeviceSDK;

namespace DeMasterProCloud.Service
{
    public interface IFirmwareVersionService
    {
        List<FirmwareVersionListModel> Gets(FirmwareVersionFilterModel filter, out int recordsFiltered, out int recordsTotal);
        bool Add(FirmwareVersionModel model);
        void Delete(List<int> ids);
        FirmwareVersion GetById(int id);
        void UpdateFirmwareToDevices(FirmwareVersion firmwareVersion, List<string> processIds);
        bool CheckHashFirmware(string fileName, string hash);
    }

    public class FirmwareVersionService : IFirmwareVersionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IWebSocketService _webSocketService;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;
        private readonly SDKSettingModel _sdkConfig;

        public FirmwareVersionService(IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor contextAccessor, IWebSocketService webSocketService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _webSocketService = webSocketService;
            _httpContext = contextAccessor.HttpContext;
            _mapper = MapperInstance.Mapper;
            _sdkConfig = configuration.GetSection(Constants.SDKDevice.DefineConfig).Get<SDKSettingModel>();
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<FirmwareVersionService>();
        }

        public List<FirmwareVersionListModel> Gets(FirmwareVersionFilterModel filter, out int recordsFiltered, out int recordsTotal)
        {
            try
            {
                var data = _unitOfWork.FirmwareVersionRepository.Gets();
                recordsTotal = data.Count();

                if (!string.IsNullOrEmpty(filter.Version))
                {
                    data = data.Where(m => m.Version.ToLower().Contains(filter.Version));
                }
                if (filter.DeviceTypes != null && filter.DeviceTypes.Any())
                {
                    data = data.Where(m => filter.DeviceTypes.Contains(m.DeviceType));
                }

                recordsFiltered = data.Count();
                data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
                data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);

                return data.Select(_mapper.Map<FirmwareVersionListModel>).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Gets");
                recordsTotal = 0;
                recordsFiltered = 0;
                return new List<FirmwareVersionListModel>();
            }
        }

        public bool Add(FirmwareVersionModel model)
        {
            try
            {
                var data = _mapper.Map<FirmwareVersion>(model);
                _unitOfWork.FirmwareVersionRepository.Add(data);
                _unitOfWork.Save();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
                return false;
            }
        }

        public void Delete(List<int> ids)
        {
            try
            {
                var data = _unitOfWork.FirmwareVersionRepository.Gets(m => ids.Contains(m.Id));
                _unitOfWork.FirmwareVersionRepository.DeleteRange(data);
                _unitOfWork.Save();

                foreach (var item in data)
                {
                    if (File.Exists(item.FileName))
                    {
                        File.Delete($"{Constants.Settings.DefineFolderFirmwareVersion}/{item.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        public FirmwareVersion GetById(int id)
        {
            try
            {
                return _unitOfWork.FirmwareVersionRepository.GetById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        public void UpdateFirmwareToDevices(FirmwareVersion firmwareVersion, List<string> processIds)
        {
            string undefinedChar = "_";
            try
            {
                foreach (var item in processIds)
                {
                    var deviceId = item.Split("::")[0];
                    var processId = item.Split("::")[1];
                    var device = _unitOfWork.IcuDeviceRepository.Gets(m => m.Id == int.Parse(deviceId)).FirstOrDefault();
                    
                    if (device != null)
                    {
                        string firmwareType = "", targetFile = "", nameCompare = "";
                        string target = firmwareVersion.FileName.Split("_")[0];

                        switch (device.DeviceType)
                        {
                            case (short)DeviceType.Icu300N:
                            case (short)DeviceType.Icu300NX:
                            case (short)DeviceType.Icu400:
                            {
                                // Make protocol - Main firmware file
                                if (target.Equals(DeviceType.Icu300N.GetDescription()))
                                {
                                    nameCompare = DeviceType.Icu300N.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = IcuFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                else if (target.Equals(DeviceType.Icu400.GetDescription()))
                                {
                                    nameCompare = DeviceType.Icu400.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = IcuFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                // Make protocol - Reader0 firmware file
                                var cardReader0 = !string.IsNullOrEmpty(device.VersionReader0) ? device.VersionReader0.Split("_")[0] : string.Empty;
                                if (cardReader0.Equals(target))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = device.RoleReader0 == (short)RoleRules.In ? IcuFileType.InReader.GetDescription() : IcuFileType.OutReader.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                // Make protocol - Reader1 firmware file
                                var cardReader1 = !string.IsNullOrEmpty(device.VersionReader1) ? device.VersionReader1.Split("_")[0] : string.Empty;
                                if (cardReader1.Equals(target, StringComparison.OrdinalIgnoreCase))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = device.RoleReader1 == (short)RoleRules.In ? IcuFileType.InReader.GetDescription() : IcuFileType.OutReader.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.ITouchPop:
                            {
                                if (target.Equals(DeviceUpdateTarget.ITouchPop2A.GetDescription()))
                                {
                                    nameCompare = DeviceType.ITouchPop.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.ITouchPop2A.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                if (DeviceUpdateTarget.Abcm.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.Abcm.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                var extraVersion = !string.IsNullOrEmpty(device.ExtraVersion) ? device.ExtraVersion.Split("_")[0] : string.Empty;
                                if (extraVersion.Equals(target))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.SoundTrack01.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.ITouchPopX:
                            {
                                if (target.Equals(DeviceUpdateTarget.ITouchPopX.GetDescription()))
                                {
                                    nameCompare = DeviceType.ITouchPopX.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                if (DeviceUpdateTarget.Module.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.ITouchPopX.GetDescription();
                                        targetFile = DeviceUpdateTarget.ITouchPopX.GetDescription();
                                    }
                                }

                                if (DeviceUpdateTarget.Library.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Library.GetDescription();
                                        targetFile = DeviceUpdateTarget.ITouchPopX.GetDescription();
                                    }
                                }

                                if (DeviceUpdateTarget.Tar.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = DeviceType.ITouchPopX.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Tar.GetDescription();
                                        targetFile = DeviceUpdateTarget.Tar.GetDescription();
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.DQMiniPlus:
                            {
                                if (target.Equals(DeviceUpdateTarget.DQMiniPlus.GetDescription()))
                                {
                                    nameCompare = DeviceType.DQMiniPlus.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.DQMiniPlus.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                            case (short)DeviceType.IT100:
                            {
                                if (target.Equals(DeviceUpdateTarget.IT100.GetDescription()))
                                {
                                    nameCompare = DeviceType.IT100.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.IT100.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                            case (short)DeviceType.PM85:
                            {
                                if (target.Equals(DeviceUpdateTarget.PM85.GetDescription()))
                                {
                                    nameCompare = DeviceType.PM85.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.PM85.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                            case (short)DeviceType.DP636X:
                            {
                                if (target.Equals(DeviceUpdateTarget.DP636X.GetDescription()))
                                {
                                    nameCompare = DeviceType.DP636X.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                if (DeviceUpdateTarget.Module.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Module.GetDescription();
                                        targetFile = DeviceUpdateTarget.DP636X.GetDescription();
                                    }
                                    
                                }
                                
                                if (DeviceUpdateTarget.Library.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Library.GetDescription();
                                        targetFile = DeviceUpdateTarget.DP636X.GetDescription();
                                    }
                                    
                                }

                                if (DeviceUpdateTarget.Tar.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = DeviceType.DP636X.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Tar.GetDescription();
                                        targetFile = DeviceUpdateTarget.Tar.GetDescription();
                                    }
                                }

                                break;
                            }
                            case (short)DeviceType.ITouch30A:
                            {
                                if (target.Equals(DeviceUpdateTarget.ITouch30A.GetDescription()))
                                {
                                    nameCompare = DeviceType.ITouch30A.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.ITouch30A.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                if (DeviceUpdateTarget.Abcm.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.Abcm.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                var extraVersion = !string.IsNullOrEmpty(device.ExtraVersion) ? device.ExtraVersion.Split("_")[0] : string.Empty;
                                if (extraVersion.Equals(target))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.SoundTrack01.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.DF970:
                            {
                                if (target.Equals(DeviceUpdateTarget.DF970.GetDescription()))
                                {
                                    nameCompare = DeviceType.DF970.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.DF970.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.Icu500:
                            {
                                if (target.Equals(DeviceUpdateTarget.Icu500.GetDescription()))
                                {
                                    nameCompare = DeviceType.Icu500.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.Icu500.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.T2Face:
                            {
                                if (target.Equals(DeviceUpdateTarget.T2Face.GetDescription()))
                                {
                                    nameCompare = DeviceType.T2Face.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.T2Face.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.BA8300:
                            {
                                if (target.Equals(DeviceUpdateTarget.BA8300.GetDescription()))
                                {
                                    nameCompare = DeviceUpdateTarget.BA8300.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.BA8300.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.RA08:
                            {
                                if (target.Equals(DeviceUpdateTarget.RA08.GetDescription()))
                                {
                                    nameCompare = DeviceUpdateTarget.RA08.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.RA08.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                
                                break;
                            }
                            case (short)DeviceType.DQ8500:
                            {
                                if (target.Equals(DeviceUpdateTarget.DQ8500.GetDescription()))
                                {
                                    nameCompare = DeviceUpdateTarget.DQ8500.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.DQ8500.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                            case (short)DeviceType.DQ200:
                            {
                                if (target.Equals(DeviceUpdateTarget.DQ200.GetDescription()))
                                {
                                    nameCompare = DeviceUpdateTarget.DQ200.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.DQ200.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                            case (short)DeviceType.TBVision:
                            {
                                if (target.Equals(DeviceUpdateTarget.TBVision.GetDescription()))
                                {
                                    nameCompare = DeviceUpdateTarget.TBVision.GetDescription() + undefinedChar;
                                    if (firmwareVersion.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.TBVision.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                        }

                        string path = $"{Constants.Settings.DefineFolderFirmwareVersion}/{firmwareVersion.FileName}";
                        string sender = _httpContext.User.GetUsername();
                        string language = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId())?.Language;
                        byte[] fileData = File.ReadAllBytes(path);

                        if (!string.IsNullOrEmpty(firmwareType))
                        {
                            // check device is not aratek => send file transfer
                            List<short> terminalAndroids = new List<short>()
                            {
                                (short)DeviceType.BA8300,
                                (short)DeviceType.DF970,
                                (short)DeviceType.Icu500,
                                (short)DeviceType.RA08,
                                (short)DeviceType.DQ8500,
                                (short)DeviceType.DQ200,
                                (short)DeviceType.TBVision,
                                (short)DeviceType.T2Face,
                            };
                            if (!terminalAndroids.Contains(device.DeviceType))
                            {
                                // send list file
                                var dataFileTransfer = MakeDataProtocolFileTransferByByteArray(fileData, firmwareVersion.FileName, device, targetFile, firmwareType);
                                int total = dataFileTransfer.Count + 1;
                                for (int i = 1; i < total; i++)
                                {
                                    var message = new DeviceUploadFileProtocolData
                                    {
                                        MsgId = Helpers.CreateMsgIdProcess(processId, i, total),
                                        Sender = sender,
                                        Type = Constants.Protocol.FileDownLoad,
                                        Data = dataFileTransfer[i - 1],
                                    };


                                }
                                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);

                                // send device instruction
                                string linkFile = "";
                                if (device.CompanyId.HasValue)
                                {
                                    var companyOfDevice = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                                    string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                                    string textUtcNow = DateTime.UtcNow.ToString(Constants.DateTimeFormat.DdMMyyyyHH);
                                    string hash = CryptographyHelper.GetMD5Hash($"{firmwareVersion.FileName}{companyOfDevice.Code}{textUtcNow}");
                                    linkFile = $"{connectionApi}{Constants.Route.ApiFirmwareVersionDeviceDownload}?fileName={firmwareVersion.FileName}&hash={hash}";
                                }
                                deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                                {
                                    DeviceId = device.Id,
                                    DeviceAddress = device.DeviceAddress,
                                    MessageType = Constants.ActionType.UpdateDevice,
                                    Command = Constants.CommandType.UpdateFirmware,
                                    FwType = firmwareType,
                                    Target = target,
                                    Sender = sender,
                                    MessageIndex = total,
                                    MessageTotal = total,
                                    MsgId = processId,
                                    LinkFile = linkFile,
                                    FileData = fileData,
                                });
                            }
                            else
                            {
                                // device is BA8300 
                                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);

                                // send device instruction
                                string linkFile = "";
                                if (device.CompanyId.HasValue)
                                {
                                    var companyOfDevice = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                                    string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                                    string textUtcNow = DateTime.UtcNow.ToString(Constants.DateTimeFormat.DdMMyyyyHH);
                                    string hash = CryptographyHelper.GetMD5Hash($"{firmwareVersion.FileName}{companyOfDevice.Code}{textUtcNow}");
                                    linkFile = $"{connectionApi}{Constants.Route.ApiFirmwareVersionDeviceDownload}?fileName={firmwareVersion.FileName}&hash={hash}";
                                }
                                deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                                {
                                    DeviceId = device.Id,
                                    DeviceAddress = device.DeviceAddress,
                                    MessageType = Constants.ActionType.UpdateDevice,
                                    Command = Constants.CommandType.UpdateFirmware,
                                    FwType = firmwareType,
                                    Target = target,
                                    Sender = sender,
                                    MessageIndex = 1,
                                    MessageTotal = 1,
                                    MsgId = processId,
                                    LinkFile = linkFile,
                                    FileData = fileData,
                                    FileName = firmwareVersion.FileName,
                                    Version = firmwareVersion.Version,
                                });

                                // // send to SDK
                                // string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiUpdateFirmware}";
                                // string token = ApplicationVariables.SDKToken;
                                // var data = new FirmwareVersionDeviceModel
                                // {
                                //     DeviceAddress = device.DeviceAddress,
                                //     DeviceType = device.DeviceType,
                                //     Sender = sender,
                                //     RoleReader0 = device.RoleReader0,
                                //     RoleReader1 = device.RoleReader1,
                                //     VersionReader0 = device.VersionReader0,
                                //     VersionReader1 = device.VersionReader1,
                                //     ExtraVersion = device.ExtraVersion,
                                //     UrlUploadFileResponse = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>(),
                                //     LinkFile = linkFile
                                //
                                // };
                                //
                                // Helpers.UploadFileMultipartFormData(url, firmwareVersion.FileName, fileData, data, token).GetAwaiter().GetResult();
                            }
                        }
                        else
                        {
                            var topic = Constants.RabbitMq.NotificationTopic;
                            if (device.CompanyId.HasValue)
                            {
                                var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                                topic = $"{Constants.RabbitMq.NotificationTopic}.{company.Code}";
                                if (string.IsNullOrEmpty(language))
                                {
                                    language = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(company.Id).Value);
                                }
                                
                                // send message error
                                var notification = new NotificationProtocolDataDetail
                                {
                                    MessageType = Constants.MessageType.Error,
                                    NotificationType = Constants.NotificationType.FileTransferError,
                                    User = sender,
                                    Message = string.Format(
                                        MessageResource.ResourceManager.GetString("msgFileNameError",
                                            new CultureInfo(language)), nameCompare),
                                };
                                _webSocketService.SendWebSocketToFE(Constants.Protocol.Notification, company.Id,notification);
                            }
                        }
                    }

                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        public bool CheckHashFirmware(string fileName, string hash)
        {
            try
            {
                List<string> textUtcNow = new List<string>();
                for (int i = 0; i <= Constants.Settings.TimeToLiveLinkDownLoadFw; i++)
                {
                    textUtcNow.Add(DateTime.UtcNow.AddHours(-i).ToString(Constants.DateTimeFormat.DdMMyyyyHH));
                }
                var companies = _unitOfWork.CompanyRepository.Gets(m => !m.IsDeleted);
                foreach (var company in companies)
                {
                    foreach (var itemUtcNow in textUtcNow)
                    {
                        if (CryptographyHelper.VerifyMD5Hash($"{fileName}{company.Code}{itemUtcNow}", hash))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckHashFirmware");
                return false;
            }
        }

        private List<DeviceUploadFileDetail> MakeDataProtocolFileTransferByByteArray(byte[] file,string fileName, IcuDevice device, string target, string fwType)
        {
            List<DeviceUploadFileDetail> data = new List<DeviceUploadFileDetail>();
            
            if (file.Length > 0)
            {
                var maxSplitSize = Helpers.GetMaxFileSplit(device.DeviceType);
                var dataFile = FileHelpers.SplitFileByByteArray(file, maxSplitSize);
                
                if (dataFile.Count > 0)
                {
                    for (var i = 0; i < dataFile.Count; i++)
                    {
                        var fileUploadDetail = new DeviceUploadFileDetail
                        {
                            FrameIndex = i,
                            TotalIndex = dataFile.Count,
                            Extension = FileHelpers.GetFileExtensionByFileName(fileName),
                
                            Target = (target.ToLower().Equals(DeviceUpdateTarget.ITouchPop2A.GetDescription().ToLower())) 
                                     || (target.ToLower().Equals(DeviceUpdateTarget.Abcm.GetDescription().ToLower())) 
                                ? FileHelpers.GetFileNameWithoutExtensionByFileName(fileName) : target,
                            FwType = fwType,
                            FileName = target.ToLower().Equals(DeviceUpdateTarget.Tar.GetDescription().ToLower()) ? "update" : FileHelpers.GetFileNameWithoutExtensionByFileName(fileName),
                            Data = dataFile[i]
                        };
                        
                        data.Add(fileUploadDetail);
                    }
                }
            }
            
            return data;
        }

    }
}
