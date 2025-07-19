using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using  VC.AG.Models;
using  VC.AG.Models.Enums;
using  VC.AG.Models.Extensions;
using  VC.AG.Models.ValuesObject;
using  VC.AG.ServiceLayer.Contracts;
using  VC.AG.WebAPI.Models;

namespace  VC.AG.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion(AppConstants.ApiVersion)]
    [Route("api/v{version:apiVersion}/file")]
    public class FileController(IUserContract userSvc, IFileService fileSvc) : ControllerBase
    {
        [HttpPost("fetch")]
        [ProducesResponseType<DBFile>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Get([FromBody] ReqFile reqFile)
        {
            DBFile? dBFile = reqFile.ToDBFile();
            DBFile? result = fileSvc.Get(dBFile).Result;
            return Ok(result);
        }
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType<DBFile>(StatusCodes.Status200OK)]
        [RequestSizeLimit(4*8000000)] // Compliant: 8MB
        public IActionResult Post([FromForm] FileCreate reqCreate, [FromForm] IFormFileCollection Files)
        {
            var file = Files?[0];
            DBFile? dBFile = reqCreate.ToDBFile(file, userSvc);
            DBFile? result = fileSvc.Upload(dBFile).Result;
            return Ok(result?.ToBasicInfo());
        }
        [HttpDelete]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Delete([FromBody] ReqFile reqDelete)
        {
            DBFile? dBFile = reqDelete.ToDBFile();
            var result = fileSvc.Delete(dBFile).Result;
            return Ok(result);
        }
    }
}
