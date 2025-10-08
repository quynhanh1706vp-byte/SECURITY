using Microsoft.AspNetCore.Authorization;
﻿using Microsoft.AspNetCore.Mvc;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Home controller
    /// </summary>
[Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Display data for dashboash
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return Redirect("/swagger");
        }
    }
}
