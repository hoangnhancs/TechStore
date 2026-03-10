using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductService.Entities;
using Shared.Core.EF.Domain.Repositories;

namespace ProductService.Repositories.Interface
{
    public interface IFilterTagRepository : IBaseEFRepository<FilterTag, int>
    {
        
    }
}