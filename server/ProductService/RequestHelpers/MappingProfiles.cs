using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contract;
using ProductService.DTOs;
using ProductService.Entities;


namespace ProductService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
#region Product and ProductDto
            CreateMap<ProductDto, Product>();
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category == null ? "" : src.Category.Name))
                .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom(src => src.Category == null ? "" : src.Category.DisplayName))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand == null ? "" : src.Brand.Name))
                .ForMember(dest => dest.DisplayTags, opt => opt.MapFrom(src => src.DisplayTags.Select(dt => dt.DisplayTag).ToList()));

            CreateMap<CreateProductDto, Product>()
                // .ForMember(dest => dest.ProductFilterTagValues, opt => opt.MapFrom(src => src.ProductFilterTagValues.Select(ftv => new ProductFilterTagValue
                // {
                //     FilterTagValueId = int.Parse(ftv),
                // }).ToList()))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                    src.Attributes.Select((attr, attrIdx) => new ProductAttribute
                    {
                        AttributeType = attr.AttributeType,
                        Name = attr.Name,
                        Value = attr.Value,
                        DisplayOrder = attrIdx,
                    })
                .ToList()
                ));

            CreateMap<UpdateProductDto, Product>()
                // .ForMember(dest => dest.ProductFilterTagValues, opt => opt.MapFrom(src => src.ProductFilterTagValues.Select(ftv => new ProductFilterTagValue
                // {
                //     FilterTagValueId = int.Parse(ftv),
                // }).ToList()))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                    src.Attributes.Select((attr, attrIdx) => new ProductAttribute
                    {
                        AttributeType = attr.AttributeType,
                        Name = attr.Name,
                        Value = attr.Value,
                        DisplayOrder = attrIdx,
                    })
                .ToList()
                ));

            CreateMap<FilterTag, FilterTagDto>();
            CreateMap<FilterTagDto, FilterTag>();

            CreateMap<FilterTagValue, FilterTagValueDto>();
            CreateMap<FilterTagValueDto, FilterTagValue>();

            CreateMap<ProductFilterTagValue, ProductFilterTagValueDto>()
                .ForMember(dest => dest.FilterTagId, opt => opt.MapFrom(src => src.FilterTagValue!.FilterTagId));
            CreateMap<ProductFilterTagValueDto, ProductFilterTagValue>();

            CreateMap<ProductDisplayTag, ProductDisplayTagDto>();

            CreateMap<ProductAttribute, ProductAttributeDto>(); //product to dto
            CreateMap<ProductAttributeDto, ProductAttribute>(); //dto to product

            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<ProductImageDto, ProductImage>();
#endregion

#region Category and CategoryDto
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>();
#endregion


            
                // .ForMember(dest => dest.ProductFilterTagValues, opt => opt.MapFrom(src => src.FilterTags.Select(ft => new ProductFilterTagValue
                // {
                //     FilterTagValueId = ft.Value,
                // }).ToList()))
                // .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                //     src.AttributeGroups.SelectMany((ag) =>
                //         ag.Attributes.Select((attr, attrIdx) => new ProductAttribute
                //         {
                //             AttributeType = ag.GroupName,
                //             Name = attr.Name,
                //             Value = attr.Value,
                //             DisplayOrder = attrIdx,
                //         })
                //     ).ToList()
                // ));
#region Product and Contract
            CreateMap<Product, ProductCreated>();
            CreateMap<Product, ProductUpdated>();
            CreateMap<ProductAttribute, ProductAttr>(); //product to contract
            CreateMap<ProductFilterTagValue, ProductFTV>(); //product to contract
#endregion

#region Brand and BrandDto
            CreateMap<Brand, BrandDto>();
            CreateMap<BrandDto, Brand>();
#endregion

#region BannerImage and BannerImageDto
            CreateMap<BannerImage, BannerImageDto>();
            CreateMap<BannerImageDto, BannerImage>();
#endregion
        }
    }
}