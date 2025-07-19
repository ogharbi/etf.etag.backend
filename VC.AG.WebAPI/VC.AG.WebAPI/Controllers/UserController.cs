using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VC.AG.Models;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.WebAPI.Models;

namespace VC.AG.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion(AppConstants.ApiVersion)]
    [Route("api/v{version:apiVersion}/user")]
    public class UserController(IUserContract svc) : ControllerBase
    {
        [HttpGet("me")]
        [ProducesResponseType<UserEntity>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Me([FromQuery] bool force)
        {
            var result = svc.GetMe(force).Result;
            return result != null ? Ok(result) : throw new ArgumentNullException(nameof(force), "Unable to get profil");
        }
        [HttpGet("search")]
        [ProducesResponseType<IEnumerable<UserEntity>>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Search([FromQuery] string word)
        {
            var result = svc.Search(word).Result;
            return Ok(result);
        }
        [HttpGet]
        [ProducesResponseType<UserEntity>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Get([FromQuery] string email, [FromQuery] bool force)
        {
            var result = svc.Get(email, force).Result;
            return Ok(result);
        }
        [HttpPost("assign")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Assign([FromBody] ReqAssignAccess assign)
        {
            var r = assign.GetAssignAccess();
            var result = svc.Assign(r).Result;
            return Ok(result);
        }
    }
}
