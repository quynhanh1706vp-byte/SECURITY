using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Vehicle;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeMasterProCloud.Api.Controllers
{
    [Produces("application/json")]
    [CheckAddOn(Constants.PlugIn.VehiclePlugIn)]
    public class VehicleController : Controller
    {
        private readonly HttpContext _httpContext;
        private readonly IUserService _userService;
        private readonly IVisitService _visitService;
        private readonly IVehicleService _vehicleService;
        private readonly IMapper _mapper;

        public VehicleController(IHttpContextAccessor httpContextAccessor, IUserService userService, IVisitService visitService, 
            IVehicleService vehicleService, IMapper mapper)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _userService = userService;
            _visitService = visitService;
            _vehicleService = vehicleService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list vehicels of users
        /// </summary>
        /// <param name="search"> search plate number or owner or mode or color </param>
        /// <param name="isUser">If true: get list vehicles of all uses, else false: get list vehicles of all visit</param>
        /// <param name="pageNumber">pageNumber</param>
        /// <param name="pageSize">pageSize</param>
        /// <param name="sortColumn">sortColumn</param>
        /// <param name="sortDirection">sortDirection</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetListVehiclesOfUser(string search, bool isUser, int pageNumber = 0, int pageSize = 10, string sortColumn = "PlateNumber", string sortDirection = "asc")
        {
            var vehicles = _vehicleService.GetListVehiclesOfUser(search, isUser, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered);
            
            var pagingData = new PagingData<VehicleListModel>
            {
                Data = vehicles,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        /// <summary>
        /// Get vehicle by id
        /// </summary>
        /// <param name="id">vehicle id</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVehicleId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetVehiclesById(int id)
        {
            var vehicle = _vehicleService.GetVehiclesById(id);
            if (vehicle == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVehicle);
            return Ok(vehicle);
        }
        /// <summary>
        /// Get list vehicles by user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersIdVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetListVehiclesByUser(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);

            var vehicles = _vehicleService.GetListVehiclesByUser(id);
            if (vehicles.Any())
            {
                return Ok(vehicles.AsEnumerable().Select(_mapper.Map<VehicleModel>).ToList());
            }
            else
            {
                return Ok(new List<VehicleModel>());
            }
        }

        /// <summary>
        /// add new vehicle of user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersIdVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult AddByUser(int id, [FromBody] VehicleModel model)
        {
            model.Id = 0;

            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var user = _userService.GetById(id);
            if (user == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);

            if (!string.IsNullOrWhiteSpace(model.PlateNumber))
            {
                model.PlateNumber = model.PlateNumber.RemoveAllEmptySpace();
            }

            var vehicle = _vehicleService.GetByPlateNumber(model.PlateNumber, _httpContext.User.GetCompanyId());
            if (vehicle != null)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, CommonResource.lblVehicle));
            }

            _vehicleService.AddForUser(id, model);
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageAddSuccess, CommonResource.lblVehicle));
        }

        /// <summary>
        ///update vehicle by user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="vehicleId">vehicle id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersIdVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult UpdateByUser(int id, int vehicleId, [FromBody] VehicleModel model)
        {

            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            var user = _userService.GetById(id);
            if (user == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);

            var vehicle = _vehicleService.GetByVehicleId(vehicleId);
            if (vehicle == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVehicle);

            model.Id = vehicleId;

            if (!string.IsNullOrWhiteSpace(model.PlateNumber))
            {
                model.PlateNumber = model.PlateNumber.RemoveAllEmptySpace();
            }

            _vehicleService.EditPersonalVehicle(model);

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, CommonResource.lblVehicle, ""));
        }

        /// <summary>
        /// delete vehicle by user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="vehicleId">vehicle id</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersIdVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult DeleteByUser(int id, int vehicleId)
        {
            var user = _userService.GetById(id);
            if (user == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);

            var vehicle = _vehicleService.GetByVehicleId(vehicleId);
            if (vehicle == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVehicle);

            _vehicleService.Delete(vehicleId);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, CommonResource.lblVehicle));
        }

        /// <summary>
        /// Get list vehicles by visitor id
        /// </summary>
        /// <param name="id">visitor id</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsIdVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult GetListVehiclesByVisit(int id)
        {
            var visit = _visitService.GetById(id);
            if (visit == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);

            var vehicles = _vehicleService.GetListVehiclesByVisit(id);
            
            return Ok(vehicles);
        }

        /// <summary>
        /// add new vehicle of visitor id
        /// </summary>
        /// <param name="id">visitor id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsIdVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult AddByVisit(int id, [FromBody] VehicleModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
            var visit = _visitService.GetById(id);
            if (visit == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);

            if (!string.IsNullOrWhiteSpace(model.PlateNumber))
            {
                model.PlateNumber = model.PlateNumber.RemoveAllEmptySpace();
            }

            var vehicle = _vehicleService.GetByPlateNumber(model.PlateNumber, _httpContext.User.GetCompanyId());
            if (vehicle != null)
            {
                bool isExist = true;
                if (vehicle.VisitId.HasValue)
                {
                    var visitVehicle = _visitService.GetById(vehicle.VisitId.Value);
                    if (visitVehicle.EndDate < DateTime.Now)
                    {
                        _vehicleService.Delete(vehicle.Id);
                        isExist = false;
                    }
                }
                if (isExist)
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, CommonResource.lblVehicle));
            }

            _vehicleService.AddForVisit(id, model);
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageAddSuccess, CommonResource.lblVehicle));
        }

        /// <summary>
        /// update vehicle by visitor id
        /// </summary>
        /// <param name="id">visitor id</param>
        /// <param name="vehicleId">vehicle id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsIdVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult UpdateByVisit([FromBody] VehicleModel model, int id, int vehicleId)
        {
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            var visit = _visitService.GetById(id);
            if (visit == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);

            // check role aprroval
            int accountId = _httpContext.User.GetAccountId();
            var visitSetting = _visitService.GetVisitSettingCompany();
            List<int> accountEdited = new List<int>()
            {
                visit.CreatedBy,
                visit.ApproverId1,
                visit.ApproverId2
            };
            if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
            {
                switch (visit.Status)
                {
                    case (short)VisitChangeStatusType.Waiting:
                        {
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved1:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                }
            }
            else if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
            {
                switch (visit.Status)
                {
                    case (short)VisitChangeStatusType.Waiting:
                        {
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            else if (accountId == visit.ApproverId2)
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, VisitResource.msgRequiredFirstApproval);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved1:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            accountEdited.Remove(visit.ApproverId1);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.AutoApproved:
                        {
                            accountEdited.Remove(visit.ApproverId1);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                }
            }

            if (!string.IsNullOrWhiteSpace(model.PlateNumber))
            {
                model.PlateNumber = model.PlateNumber.RemoveAllEmptySpace();
            }

            var vehicle = _vehicleService.GetByVehicleId(vehicleId);
            if (vehicle == null)
            {
                //return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
                vehicleId = _vehicleService.AddForVisit(id, model);
            }
            else
            {
                _vehicleService.Update(model, vehicleId);
            }

            // _vehicleService.Update(model, vehicleId);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, CommonResource.lblVehicle, ""));
        }

        /// <summary>
        /// delete vehicle by visit id
        /// </summary>
        /// <param name="id">visit id</param>
        /// <param name="vehicleId">vehicle id</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsIdVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult DeleteByVisit(int id, int vehicleId)
        {
            var visit = _visitService.GetById(id);
            if (visit == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);

            var vehicle = _vehicleService.GetByVehicleId(vehicleId);
            if (vehicle == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVehicle);

            _vehicleService.Delete(vehicleId);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, CommonResource.lblVehicle));
        }


        /// <summary>
        /// Get personal vehicle information.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUserVehicleId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetPersonalVehicles(int id)
        {
            var model = new PersonalVehicleOptionModel();
            if (id != 0)
            {
                var vehicle = _vehicleService.GetAllByVehicleId(id);
                if (vehicle == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVehicle);
                }
                model = _mapper.Map<PersonalVehicleOptionModel>(vehicle);
            }

            return Ok(model);
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUserVehicleId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ApprovePersonalVehicle(int id)
        {
            var personalVehicle = _vehicleService.GetByVehicleId(id);

            if (personalVehicle == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVehicle);
            }
            else if (_httpContext.User.GetAccountType() != (int)AccountType.PrimaryManager)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, string.Format(MessageResource.msgNotPermission));
            }

            _vehicleService.ApprovePersonalVehicle(personalVehicle);

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.ApprovedSuccessfully));
        }

        /// <summary>
        /// Delete multi vehicle
        /// </summary>
        /// <param name="ids">list vehicle id</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVehicles)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteVehicle(List<int> ids)
        {
            var unitVehicles = _vehicleService.GetPersonalVehicleByVehicleIds(ids);

            if (!ids.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.NotSelected, CommonResource.lblVehicle));
            }

            if (unitVehicles == null || !unitVehicles.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.RecordNotFound));
            }

            _vehicleService.DeletePersonalVehicleMulti(unitVehicles);

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, CommonResource.lblVehicle, ""));
        }
        
         /// <summary>
        /// Import vehicles data
        /// </summary>
        /// <param name="file">file include list of vehicles to import</param>
        /// <param name="type">type of file</param>
        /// <param name="isUser">import vehicles for user or visit</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Data of file imported wrong. Or extension file wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiVehiclesImport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ImportUser(IFormFile file, string type = "excel", bool isUser = true)
        {
            if (file.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgFailLengthFile);
            }
            var fileType = FileHelpers.GetFileExtension(file);
            List<string> extensions = new List<string>();

            switch (type.ToLower())
            {
                case "excel":
                    extensions.Add(".xls");
                    extensions.Add(".xlsx");
                    break;
                //case "hancell":
                //extensions.Add(".cell");
                //break;
                default:
                    extensions.Add(".txt");
                    extensions.Add(".csv");
                    break;
            }

            if (!extensions.Contains(fileType.ToLower()))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(MessageResource.msgErrorFileExtension, string.Join(" or ", extensions)));
            }

            try
            {
                // Validate file headers before processing
                var headerValidation = _vehicleService.ValidateImportFileHeaders(file);
                if (!headerValidation.Result)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, headerValidation.Message);
                }

                var stream = FileHelpers.ConvertToStream(file);
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();
                var accountName = _httpContext.User.GetUsername();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var result = await _vehicleService.ImportFile(type, stream, companyId, accountId, accountName, isUser);
                        if (!result.Result)
                        {
                            Console.WriteLine($"Import failed: {result.Message}");
                        }
                        else
                        {
                            Console.WriteLine("Import completed successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing vehicle: {ex}");
                    }
                });
                
                return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgUserImportSuccess);
            }
            catch (Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, "ERROR to import vehicles.");
            }
        }
         
        /// <summary>
        /// Get file template import vehicle
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isUser"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiGetVehicleTemplate)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetFileImportVehicleTemplate(string type = "excel", bool isUser = true)
        {
            int companyId = _httpContext.User.GetCompanyId();
            byte[] data;
            switch (type)
            {
                default:
                    data = _vehicleService.GetFileExcelImportVehicleTemplate(companyId, isUser);
                    break;
            }
            
            return File(data, type.Equals("excel") ? "application/ms-excel" : "application/csv", 
                type.Equals("excel") ? "Vehicle_Template.xlsx" : "VehicleTemplate.csv");
        }
        /// <summary>
        /// Export vehicle for user or visit
        /// </summary>
        /// <param name="type">Type of file export</param>
        /// <param name="isUser">True is User, False is Visit</param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiExportVehicle)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ExportVehicle(string type = "excel", bool isUser = true)
        {
            try
            {
                int companyId = _httpContext.User.GetCompanyId();
                var fileData = _vehicleService.Export(type, isUser, companyId, out var totalRecords, out var recordsFiltered);
                var filename = string.Format(Constants.ExportFileFormat, UserResource.lblExport + "_Vehicle_" + (isUser ? "User" : "Visit"), DateTime.Now);

                string extension;
                string fileMIME;

                switch (type.ToLower())
                {
                    case "excel":
                        extension = "xlsx";
                        fileMIME = "application/ms-excel";
                        break;
                    case "hancell":
                        extension = "cell";
                        fileMIME = "application/octet-stream";
                        break;
                    default:
                        extension = "csv";
                        fileMIME = "text/csv";
                        break;
                }

                string fullName = $"{filename}.{extension}";

                if (totalRecords == 0 || recordsFiltered == 0)
                {
                    return new ApiSuccessResult(StatusCodes.Status200OK,
                        string.Format(MessageResource.MessageExportDataIsZero));
                }

                return File(fileData, fileMIME, fullName);
            }
            catch (Exception)
            {
                // There was not the catch for "Throw" in _userService.Export.
                return new ApiSuccessResult(StatusCodes.Status422UnprocessableEntity, $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Fail}");
            }
        }
    }
}