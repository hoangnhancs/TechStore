using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Data;
using IdentityService.Repositories.Interfaces;
using Shared.Core.EF.UnitOfWork;

namespace IdentityService.Persistence
{
    public interface IIdentityUnitOfWork : IUnitOfWork
    {
        IRefreshTokenRepository RefreshTokenRepository { get; }
        IAddressRepository AddressRepository { get; }
        IUserImageRepository UserImageRepository { get; }
    }
}