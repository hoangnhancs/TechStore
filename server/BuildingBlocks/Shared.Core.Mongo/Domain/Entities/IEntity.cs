using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Core.Mongo.Domain.Entities
{
    public interface IEntity<TId>
    {
        TId Id { get; }
    }
}