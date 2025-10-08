using System;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Protocol;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Constants = DeMasterProCloud.Common.Infrastructure.Constants;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Service
{
    public interface ISendMessageService
    {
        void SendDeviceStatusToFE(IcuDevice device, string doorStatus = "", IModel channel = null, IUnitOfWork unitOfWork = null);
    }

    public class SendMessageService : ISendMessageService
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public SendMessageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _mapper = MapperInstance.Mapper;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<SendMessageService>();
        }

        public void SendDeviceStatusToFE(IcuDevice device, string doorStatus = "", IModel channel = null, IUnitOfWork unitOfWork = null)
        {
            try
            {
                var data = _mapper.Map<IcuDevice, DeviceStatusDetail>(device);
                data.DoorStatus = doorStatus == "" ? null : doorStatus;
                data.FromDbIdNumber = unitOfWork != null
                    ? unitOfWork.CardRepository.GetCardAvailableInDevice(device.Id).Count()
                    : 0;

                var protocolData = new SDKDataWebhookModel()
                {
                    Type = Constants.SDKDevice.WebhookDeviceConnectionType,
                    Data = data
                };

                ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(protocolData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendDeviceStatusToFE");
            }
        }
    }
}

