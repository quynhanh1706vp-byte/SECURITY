using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Device;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DeMasterProCloud.Repository
{
    public interface ICameraRepository : IGenericRepository<Camera>
    {
        List<CameraListModel> GetCamerasByCompany(int companyId);
        List<Camera> GetCameraByIcuDeviceId(int icuDeviceId);
        Camera GetByCameraId(string cameraId);
        List<Camera> GetByCompanyIdAndIds(int companyId, List<int> ids);
        List<Camera> GetCameraByType(List<int> types);
        void ChangedConnectionStatus(Camera camera, short status);
    }
    
    public class CameraRepository : GenericRepository<Camera>, ICameraRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public CameraRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<CameraRepository>();
        }

        public List<CameraListModel> GetCamerasByCompany(int companyId)
        {
            var data = from c in _dbContext.Camera
                join d in _dbContext.IcuDevice on c.IcuDeviceId equals d.Id into g
                from d1 in g.DefaultIfEmpty()
                where companyId == 0 || c.CompanyId == companyId
                select new CameraListModel()
                {
                    Id = c.Id,
                    Name = c.Name,
                    PlaceID = c.PlaceID,
                    IcuId = c.IcuDeviceId ?? 0,
                    CompanyId = c.CompanyId,
                    DeviceName = d1.Name,
                    CameraId = c.CameraId,
                    VideoLength = c.VideoLength,
                    Type = c.Type,
                    ConnectionStatus = c.ConnectionStatus,
                    // AutoOpenDoor = c.AutoOpenDoor,
                    // TimeOpenDoor = c.TimeOpenDoor,
                    RoleReader = c.RoleReader,
                    SaveEventUnknownFace = c.SaveEventUnknownFace,
                    CheckEventFromWebHook = c.CheckEventFromWebHook,
                    BuildingId = d1.BuildingId,
                    UrlStream = c.UrlStream,
                    VmsUrlStream = c.VmsUrlStream,
                    Similarity = c.Similarity,
                    VoiceAlarm = c.VoiceAlarm,
                    LightAlarm = c.LightAlarm,
                };

            return data.ToList();
        }

        public List<Camera> GetCameraByIcuDeviceId(int icuDeviceId)
        {
            return _dbContext.Camera.Where(c => c.IcuDeviceId == icuDeviceId).ToList();
        }

        public Camera GetByCameraId(string cameraId)
        {
            return _dbContext.Camera.Include(c => c.IcuDevice).FirstOrDefault(c => c.CameraId == cameraId);
        }

        public List<Camera> GetByCompanyIdAndIds(int companyId, List<int> ids)
        {
            var cameras = _dbContext.Camera.Where(m => (!ids.Any() || ids.Contains(m.Id)) && m.CompanyId == companyId).ToList();
            return cameras;
        }

        public List<Camera> GetCameraByType(List<int> types)
        {
            return _dbContext.Camera.Where(m => types.Contains(m.Type)).ToList();
        }

        public void ChangedConnectionStatus(Camera camera, short status)
        {
            bool isOnline = status == (short)ConnectionStatus.Online;

            if (isOnline)
            {
                camera.LastCommunicationTime = DateTime.UtcNow;
            }
            if (camera.ConnectionStatus == status)
            {
                try
                {
                    _dbContext.Camera.Update(camera);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ChangedConnectionStatus");
                }
                return;
            }

            camera.ConnectionStatus = status;

            if (camera.IcuDeviceId.HasValue)
            {
                var eventLog = new EventLog
                {
                    IcuId = camera.IcuDeviceId.Value,
                    Camera = camera,
                    CameraId = camera.Id,
                    DoorName = camera.Name,
                    CompanyId = camera.CompanyId,
                    EventType = (int)(isOnline ? EventType.CommunicationSucceed : EventType.CommunicationFailed),
                    EventTime = DateTime.UtcNow
                };
                try
                {
                    _dbContext.EventLog.Add(eventLog);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ChangedConnectionStatus");
                }
            }

            try
            {
                _dbContext.Camera.Update(camera);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangedConnectionStatus");
            }
        }
    }
}