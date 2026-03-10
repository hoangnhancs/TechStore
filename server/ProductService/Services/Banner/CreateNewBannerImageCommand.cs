using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Banner
{
    public class CreateNewBannerImageCommand : IRequest<AppResult<List<BannerImageDto>>>     
    {
        public required List<IFormFile> NewImages { get; set; }
    }
}