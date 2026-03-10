using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductService.Services.FilterTag;
using Shared.Web.Controller;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterTagController : BaseApiController
    {
        // [HttpGet]
        // [AllowAnonymous]
        // public async Task<IActionResult> GetFilterTags()
        // {
        //     return HandleAppResult(await Mediator.Send(new GetAllFilterTagQuery()));
        // }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetFilterTags([FromQuery] int? catId)
        {
            return HandleAppResult(await Mediator.Send(new GetAllFilterTagQuery{ CategoryId = catId }));
        }
    }
}