using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using  VC.AG.Models;
using  VC.AG.ServiceLayer.Contracts;
using  VC.AG.WebAPI.Models;
using  VC.AG.Models.Extensions;
using static  VC.AG.Models.AppConstants;
using Microsoft.AspNetCore.Http.HttpResults;
using  VC.AG.Models.ValuesObject;
using  VC.AG.Models.Enums;
using  VC.AG.Models.Entities;

namespace  VC.AG.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion(AppConstants.ApiVersion)]
    [Route("api/v{version:apiVersion}/form")]
    public class FormController(IUserContract userSvc, IFormContract formSvc) : ControllerBase
    {
        [HttpPost("fetch")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Get([FromBody] FormFetch req)
        {
            var user = userSvc.GetMe().Result;
            var query = req.ToDGQuery(user);
            var stream = formSvc.Get(query, req.Site).Result;
            var result = stream?.SerializeItem();
            return stream != null ? Ok(result) : Ok(null);
        }
        [HttpPost("requests")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Get([FromBody] ReqForms req)
        {
            var user = userSvc.GetMe().Result;
            var query = req.ToFormQuery(user);
            var stream = formSvc.GetAll(query, req.Site).Result;
            var result = stream?.SerializeStream();
            return stream != null ? Ok(result) : Ok(null);
        }
        [HttpPost("filterValues")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult GetFilterValues([FromBody] ReqForms req)
        {
            var user = userSvc.GetMe().Result;
            var query = req.ToFormQuery(user);
            var result = formSvc.GetFilterValues(query, req.Site).Result;
            return result != null ? Ok(result) : Ok(null);
        }
        [HttpPost]
        [ProducesResponseType<DBItem>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Post([FromBody] ReqCreate reqCreate)
        {

            var d = reqCreate.ToDBCreate(userSvc);
            var result = formSvc.Post(d).Result;
            return Ok(result);
        }
        [HttpPut]
        [ProducesResponseType<DBItem>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Put([FromBody] ReqUpdate reqUpdate)
        {
            var d = reqUpdate.ToDBUpdate(userSvc);
            var result = formSvc.Put(d).Result;
            return Ok(result);
        }
        [HttpPost("delete")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Delete([FromBody] ReqUpdate reqUpdate)
        {
            var d = reqUpdate.ToDBUpdate(userSvc, false);
            var result = formSvc.Delete(d).Result;
            return Ok(result);
        }
        [HttpPost("ressouces")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Ressources([FromBody] ReqForms req)
        {
            var user = userSvc.GetMe().Result;
            var query = req.ToFormQuery(user);
            var stream = formSvc.Ressources(query, req.Site).Result;
            var result = stream?.SerializeStream();
            return stream != null ? Ok(result) : Ok(null);
        }
       
    }
}
