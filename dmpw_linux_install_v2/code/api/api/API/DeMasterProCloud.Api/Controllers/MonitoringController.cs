using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Service;
using AutoMapper;
using DeMasterProCloud.DataModel.AccessGroup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Monitoring controller
    /// </summary>
    [Produces("application/json")]
    [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MonitoringController : Controller
    {
        private readonly IDeviceService _deviceService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Report controller
        /// </summary>
        /// <param name="deviceService"></param>
        public MonitoringController(IDeviceService deviceService, IMapper mapper)
        {
            _deviceService = deviceService;
            _mapper = mapper;
        }

        /// <summary>
        /// Initial door list. 
        /// If the number of device is more than 5, return null.
        /// If the number of device is less than 5, return devices information.
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiMonitorsDoorList)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetDoorList()
        {
            var door = _deviceService.GetDoorList();
            var model = _mapper.Map<IEnumerable<DoorListModel>>(door);
            return Ok(model);
        }
    }
}
