using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services.Product;
using Shared.Web.Controller;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : BaseApiController
    {
        // [AllowAnonymous]
        // [HttpGet("{categoryId?}/{brandId?}")]
        // public async Task<IActionResult> GetProductsByCategory([FromRoute] int? categoryId, [FromRoute] int? brandId)
        // {
        //     return HandleAppResult(await Mediator.Send(new GetProductListByCategoryQuery { CategoryId = categoryId, BrandId = brandId }));
        // }

        [AllowAnonymous]
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductDetails([FromRoute] string productId)
        {
            return HandleAppResult(await Mediator.Send(new GetProductDetailsQuery { ProductId = productId }));
        }

        [AllowAnonymous]
        [HttpGet("all")]
        public async Task<IActionResult> GetListProduct([FromQuery] string? lastUpdated)
        {
            return HandleAppResult(await Mediator.Send(new GetProductListQuery { LastUpdated = lastUpdated }));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto)
        {      
            return HandleAppResult(await Mediator.Send(new CreateNewProductCommand { ProductDto = dto }));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct([FromForm] UpdateProductDto dto, [FromRoute] string productId)
        {
            return HandleAppResult(await Mediator.Send(new UpdateProductCommand { ProductDto = dto, ProductId = productId }));
        }
    }
}
