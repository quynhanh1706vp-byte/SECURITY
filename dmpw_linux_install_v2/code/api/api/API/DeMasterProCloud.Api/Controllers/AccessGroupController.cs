using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeMasterProCloud.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // tường minh, dù đã có AuthorizeFilter global
[IgnoreAntiforgeryToken]
    public class AccessGroupController : Controller
    {
        // GET công khai thật sự cần mở:
        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult Get(int id) => Ok(new { id });

        // POST/PUT/PATCH/DELETE: không AllowAnonymous
        [HttpPost]
        public IActionResult Create([FromBody] object dto) => Ok(dto);
    }
}
