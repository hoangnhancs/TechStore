using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductService.Services.Brand;
using Shared.Web.Controller;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetBrands([FromQuery] int? catId = null)
        {
            return HandleAppResult(await Mediator.Send(new GetAllBrandsQuery { CategoryId = catId }));
        }
    }
}