using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Core.EF.Application;

namespace ProductService.Services.Banner
{
    public class DeleteBannerImageCommand : IRequest<AppResult<Unit>>
    {
        public List<int> BannerImageIds { get; set; } = [];
    }
}