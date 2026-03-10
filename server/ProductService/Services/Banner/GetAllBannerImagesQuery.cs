using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using ProductService.DTOs;
using Shared.Core.EF.Application;

namespace ProductService.Services.Banner
{
    public class GetAllBannerImagesQuery : IRequest<AppResult<List<BannerImageDto>>>
    {
        
    }
}