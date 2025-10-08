using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Bogus.Extensions;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.DeviceReader;
using DeMasterProCloud.Repository;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// DeviceReader service interface
    /// </summary>
    public interface IDeviceReaderService
    {
        List<DeviceReaderListModel> GetPaginated(string search, List<int> buildingIds, List<int> deviceTypeIds, List<int> deviceIds,
            List<int> statusIds, int companyId, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        bool Add(DeviceReaderModel model);
        bool Update(DeviceReaderModel model, DeviceReader deviceReader);
        bool Delete(DeviceReader deviceReader);
        List<DeviceReader> GetByIds(List<int> ids);
        DeviceReader GetById(int id);
        bool DeleteRange(List<DeviceReader> deviceReaders);
        bool IsExistedName(int deviceReaderId, string name);
        byte[] ExportToFileExcel(int companyId);
    }

    public class DeviceReaderService : IDeviceReaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DeviceReaderService(IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor, ILogger<DeviceReaderService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _mapper = MapperInstance.Mapper;
        }

        public bool Add(DeviceReaderModel model)
        {
            var isSuccess = true;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var deviceReader = _mapper.Map<DeviceReader>(model);

                        _unitOfWork.DeviceReaderRepository.Add(deviceReader);
                        _unitOfWork.Save();


                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });

            return isSuccess;
        }

        public bool Update(DeviceReaderModel model, DeviceReader deviceReader)
        {
            var isSuccess = true;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Update deviceReader
                        _mapper.Map(model, deviceReader);
                        _unitOfWork.DeviceReaderRepository.Update(deviceReader);

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });

            return isSuccess;
        }

        public bool Delete(DeviceReader deviceReader)
        {
            var isSuccess = true;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // delete 
                        _unitOfWork.DeviceReaderRepository.Delete(deviceReader);
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });

            return isSuccess;
        }

        public bool DeleteRange(List<DeviceReader> deviceReaders)
        {
            var isSuccess = true;
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var deviceReader in deviceReaders)
                        {
                            _unitOfWork.DeviceReaderRepository.Delete(deviceReader);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });

            return isSuccess;
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="search"></param>
        /// <param name="buildingIds"></param>
        /// <param name="deviceTypeIds"></param>
        /// <param name="deviceIds"></param>
        /// <param name="statusIds"></param>
        /// <param name="companyId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<DeviceReaderListModel> GetPaginated(string search, List<int> buildingIds, List<int> deviceTypeIds, List<int> deviceIds,
            List<int> statusIds, int companyId, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var data = _unitOfWork.AppDbContext.DeviceReader
                    .Include(m => m.IcuDevice)
                    .ThenInclude(m => m.Building)
                    .Where(m => m.IcuDevice.CompanyId == companyId).AsQueryable();

                totalRecords = data.Count();

            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => x.Name.RemoveDiacritics().ToLower().Contains(normalizedSearch)).AsQueryable();
            }

            if (buildingIds is { Count: > 0 })
            {
                data = data.Where(x => buildingIds.Contains(x.IcuDevice.BuildingId ?? 0));
            }

            if (deviceTypeIds is { Count: > 0 })
            {
                data = data.Where(x => deviceTypeIds.Contains(x.IcuDevice.DeviceType));
            }
            
            if (deviceIds is { Count: > 0 })
            {
                data = data.Where(x => deviceIds.Contains(x.IcuDeviceId));
            }

            if (statusIds is { Count: > 0 })
            {
                data = data.Where(x => statusIds.Contains(x.IcuDevice.ConnectionStatus));
            }

            recordsFiltered = data.Count();
            
            // device reader
            List<DeviceReaderListModel> result = new List<DeviceReaderListModel>();
            result.AddRange(data.AsEnumerable().Select(_mapper.Map<DeviceReaderListModel>));
            
            // camera
            var cameras = _unitOfWork.AppDbContext.Camera
                .Include(m => m.IcuDevice)
                .Include(m => m.IcuDevice).ThenInclude(m => m.Building)
                .AsQueryable();
            totalRecords += cameras.Count();
            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                cameras = cameras.AsEnumerable().Where(x => x.Name.RemoveDiacritics().ToLower().Contains(normalizedSearch)).AsQueryable();
            }
            if (buildingIds is { Count: > 0 })
            {
                cameras = cameras.Where(x => buildingIds.Contains(x.IcuDevice.BuildingId ?? 0));
            }
            if (deviceIds is { Count: > 0 })
            {
                cameras = cameras.Where(m => m.IcuDeviceId.HasValue && deviceIds.Contains(m.IcuDeviceId.Value));
            }
            if (deviceTypeIds is { Count: > 0 })
            {
                cameras = cameras.Where(x => deviceTypeIds.Contains(x.IcuDevice.DeviceType));
            }
            if (statusIds is { Count: > 0 })
            {
                cameras = cameras.Where(x => statusIds.Contains(x.IcuDevice.ConnectionStatus));
            }
            recordsFiltered += cameras.Count();
            result.AddRange(cameras.AsEnumerable().Select(_mapper.Map<DeviceReaderListModel>));
            
            result = result.AsQueryable().OrderBy($"{sortColumn} {sortDirection}").ToList();
            if(pageSize > 0)
                result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                int index = 1;
                result.ForEach(m => m.Id = index++);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<DeviceReaderListModel>();
            }
        }


        /// <summary>
        /// Get list of deviceReader by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<DeviceReader> GetByIds(List<int> ids)
        {
            try
            {
                return _unitOfWork.DeviceReaderRepository.GetMany(m => ids.Contains(m.Id))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<DeviceReader>();
            }
        }

        public DeviceReader GetById(int id)
        {
            try
            {
                return _unitOfWork.DeviceReaderRepository.Get(m => m.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }


        /// <summary>
        /// Check if deviceReader name is exist
        /// </summary>
        /// <param name="deviceReaderId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsExistedName(int deviceReaderId, string name)
        {
            try
            {
                var deviceReader = _unitOfWork.DeviceReaderRepository.Get(m => m.Name.ToLower() == name.ToLower());
                return deviceReader != null && deviceReaderId != deviceReader.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsExistedName");
                return false;
            }
        }

        public byte[] ExportToFileExcel(int companyId)
        {
            try
            {
                var data = GetPaginated("", null, null, null, null, companyId, 0, 0, "Name", "asc", out _, out _).AsEnumerable().Select(_mapper.Map<DeviceReaderListModel>).ToList();
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("data");
                    worksheet.DefaultColWidth = 20;
                    worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    int recordIndex = 2;
                    string[] headerExcel = [
                        "Tên thiết bị",
                        "Loại thiết bị",
                        "Tên điểm kiểm soát",
                        "Đơn vị",
                    ];

                    for (var i = 0; i < headerExcel.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headerExcel[i];
                    }

                    foreach (var item in data)
                    {
                        var colIndex = 1;

                        worksheet.Cells[recordIndex, colIndex++].Value = item.Name;
                        worksheet.Cells[recordIndex, colIndex++].Value = item.DeviceType;
                        worksheet.Cells[recordIndex, colIndex++].Value = item.DeviceName;
                        worksheet.Cells[recordIndex, colIndex++].Value = item.BuildingName;

                        recordIndex++;
                    }

                    return package.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportToFileExcel");
                return null;
            }
        }
    }
}