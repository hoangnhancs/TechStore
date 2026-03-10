using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Data;
using IdentityService.Repositories;
using IdentityService.Repositories.Interfaces;
using Infrastructure.EF.UnitOfWork;

namespace IdentityService.Persistence
{
    public class IdentityUnitOfWork : UnitOfWork<IdentitySvcDbContext>, IIdentityUnitOfWork
    {
        public IRefreshTokenRepository RefreshTokenRepository { get; }
        public IAddressRepository AddressRepository { get; }
        public IdentityUnitOfWork(IdentitySvcDbContext context, 
            IRefreshTokenRepository refreshTokenRepository,
            IAddressRepository addressRepository) : base(context)
        {
            RefreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            AddressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        }
    }
}