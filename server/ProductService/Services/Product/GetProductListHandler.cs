using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Persistence;
using ProductService.Repositories.Interface;
using Shared.Core.EF.Application;
using System.Data.SqlTypes;

namespace ProductService.Services.Product;

public class GetProductListHandler : IRequestHandler<GetProductListQuery, AppResult<List<ProductDto>>>
{
    private readonly IProductUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    // private readonly ICacheService _cacheService;

    public GetProductListHandler(IMapper mapper, IProductUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        // _cacheService = cacheService;
    }

    public async Task<AppResult<List<ProductDto>>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
    {
        // var cachedProducts = await _cacheService.GetAsync<List<ProductDto>>("productList");
        // if (cachedProducts != null)
        // {
        //     Console.WriteLine("Fetching products from cache.");
        //     return AppResult<List<ProductDto>>.Success(cachedProducts);
        // }
        // else
        // {
        //     Console.WriteLine("Fetching products from database.");
        // }

        DateTime? lastUpdatedDate = null;
        if (!string.IsNullOrEmpty(request.LastUpdated))
        {
            if (DateTime.TryParse(request.LastUpdated, out var parsedDate))
            {
                lastUpdatedDate = parsedDate.ToUniversalTime();
            }
        }
        else
        {
            lastUpdatedDate = SqlDateTime.MinValue.Value; // Set to minimum value to fetch all products
        }    

        var products = await _unitOfWork.ProductRepository.GetListAsync(
            p => p.IsActive && !p.IsDeleted && (!lastUpdatedDate.HasValue || p.UpdatedAt > lastUpdatedDate.Value),
            q => q
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Attributes)
                .Include(p => p.ProductFilterTagValues)
                .ThenInclude(pftv => pftv.FilterTagValue)
                .Include(p => p.DisplayTags),
            cancellationToken
        );

        // var products = await _repository.GetAllProducts(cancellationToken);

        var productsDto = products.Select(_mapper.Map<ProductDto>).ToList();

        // if (productsDto == null || productsDto.Count == 0)
        // {
        //     return AppResult<List<ProductDto>>.Failure("No products found", 404);
        // }
        // else
        // {
        //     // await _cacheService.SetAsync("productList", productsDto, TimeSpan.FromHours(1));
        //     return AppResult<List<ProductDto>>.Success(productsDto);
        // }
        return AppResult<List<ProductDto>>.Success(productsDto);
    }
}