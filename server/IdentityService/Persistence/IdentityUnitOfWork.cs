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
        private IRefreshTokenRepository? _refreshTokenRepository;
        private IAddressRepository? _addressRepository;
        private IUserImageRepository? _userImageRepository;
        public IRefreshTokenRepository RefreshTokenRepository => 
            _refreshTokenRepository ??= new RefreshTokenRepository(_dbContext);
        public IAddressRepository AddressRepository => 
            _addressRepository ??= new AddressRepository(_dbContext);   
        public IUserImageRepository UserImageRepository =>
            _userImageRepository ??= new UserImageRepository(_dbContext);
        public IdentityUnitOfWork(IdentitySvcDbContext context) : base(context)
        {
        }
    }
}