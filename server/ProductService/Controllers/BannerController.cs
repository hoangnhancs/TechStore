using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductService.Services.Banner;
using Shared.Web.Controller;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : BaseApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBannerImages()
        {
            return HandleAppResult(await Mediator.Send(new GetAllBannerImagesQuery()));
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewBannerImage([FromForm] List<IFormFile> files)
        {
            return HandleAppResult(await Mediator.Send(new CreateNewBannerImageCommand { NewImages = files }));
        }
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBannerImage([FromBody] List<int> bannerImageIds)
        {
            return HandleAppResult(await Mediator.Send(new DeleteBannerImageCommand { BannerImageIds = bannerImageIds }));
        }
    }
}