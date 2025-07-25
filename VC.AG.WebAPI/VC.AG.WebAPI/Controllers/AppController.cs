using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System.Text.Json.Serialization;
using System.Text.Json;
using VC.AG.Models;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.Extensions;
using VC.AG.Models.Helpers;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.WebAPI.Models;
using static VC.AG.Models.AppConstants;
using Wkhtmltopdf.NetCore;

namespace VC.AG.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion(AppConstants.ApiVersion)]
    [Route("api/v{version:apiVersion}/app")]
    public class AppController(ILogger<AppController> logger, IUserContract userSvc, IAppContract appSvc, INotifContract notifSvc, IGeneratePdf generatePdf) : ControllerBase
    {
        [HttpGet("site")]
        [ProducesResponseType<SiteEntity>(StatusCodes.Status200OK)]
        public IActionResult SiteInfo([FromQuery(Name = "d")] string delegation = "", [FromQuery(Name = "f")] bool force = false)
        {
            logger.LogInformation($"Get SiteInfo");
            var result = appSvc.GetSite(delegation, force).Result;
            return result != null ? (IActionResult)Ok(result.GetBasicInfo()) : throw new ArgumentNullException($"Unable to find delegation : {delegation}");
        }
        [ProducesResponseType<SiteEntity>(StatusCodes.Status200OK)]
        [HttpGet("refreshsite")]
        public IActionResult RefreshSite([FromQuery(Name = "d")] string delegation, [FromQuery(Name = "t")] SiteRefreshTarget siteRefreshTarget)
        {
            logger.LogInformation($"Get SiteInfo");
            var d = "root".EqualsNotNull(delegation) ? string.Empty : delegation;
            var result = appSvc.RefreshSite(siteRefreshTarget, d).Result;
            return result != null ? (IActionResult)Ok(result.GetBasicInfo()) : throw new ArgumentNullException($"Unable to find delegation : {delegation}");
        }
        [HttpGet("ressource")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult GetRessource([FromQuery(Name = "r")] Ressource ressource, [FromQuery(Name = "d")] string? delegation, [FromQuery(Name = "l")] string? listName, [FromQuery(Name = "fid")] string? viewId)
        {
            var items = appSvc.GetRessource(ressource, delegation, listName, viewId).Result;
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new DoubleInfinityConverter());
            var result = System.Text.Json.JsonSerializer.Serialize(items, options);
            return !string.IsNullOrEmpty(result) ? (IActionResult)Ok(result) : Ok(null);
        }
        [HttpPost("query")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Get([FromBody] ReqQuery req)
        {
            var query = req.ToDGQuery();
            if (req.ListName.EqualsNotNull(ListNameKeys.Interview)) throw new UnauthorizedAccessException($"{req.ListName} is secured");
            var stream = appSvc.GetAll(query, req.Site).Result;
            var result = stream?.SerializeStream();
            return stream != null ? (IActionResult)Ok(result) : Ok(null);
        }
        [HttpPost]
        [ProducesResponseType<DBItem>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Post([FromBody] ReqCreate reqCreate)
        {

            var d = reqCreate.ToDBCreate(userSvc);
            var result = appSvc.Post(d).Result;
            return Ok(result);
        }
        [HttpPost("form")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult PostForm()
        {
            var d = HttpContext.Request.Form.ToDBFormData();
            appSvc.PostForm(d);
            return Ok(true);
        }
        [HttpPut]
        [ProducesResponseType<DBItem>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Put([FromBody] ReqUpdate reqUpdate)
        {

            var d = reqUpdate.ToDBUpdate(userSvc);
            var result = appSvc.Put(d).Result;
            return Ok(result);
        }
        [HttpDelete]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult Delete([FromBody] ReqUpdate reqUpdate)
        {
            var d = reqUpdate.ToDBUpdate(userSvc, false);
            var result = appSvc.Delete(d).Result;
            return Ok(result);
        }
        [HttpGet("pdf/{id}")]
        public async Task<IActionResult> GetPdf(int id)
        {
            var qp = new DBQuery() { Id = id };
            var file = await appSvc.GetPdf(generatePdf, qp);
            var stream = file?.Content != null ? new MemoryStream(file.Content) : file?.ContentStream;
            if (stream != null)
                return new FileStreamResult(stream, "application/pdf");
            else return Ok(null);
        }
      

    }
}
