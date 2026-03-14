using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contract;
using SearchService.Entities;

namespace SearchService.RequestHelpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductCreated, ProductItem>();
            CreateMap<ProductUpdated, ProductItem>();
            CreateMap<ProductAttr, ProductAttributeDto>();
            CreateMap<ProductFTV, ProductFilterTagValueDto>();
                // .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src =>
                //     src.AttributeGroups.SelectMany((ag) =>
                //         ag.Attributes.Select(attr => new ProductAttribute
                //         {
                //             Name = attr.Name,
                //             Value = attr.Value,
                //             AttributeType = ag.GroupName
                //         })
                //     ).ToList()
                // ))
                // .ForMember(dest => dest.ProductFilterTagValues, opt => opt.MapFrom(src =>
                //     src.FilterTags.Select(ft => new FilterTagValue
                //     {
                //         FilterTagId = ft.Key,
                //         Value = ft.Value.ToString()
                //     }).ToList()
                // ));
        } 
    }
}