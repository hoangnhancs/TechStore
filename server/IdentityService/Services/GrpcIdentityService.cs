using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Grpc;
using Grpc.Core;
using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf.WellKnownTypes;

namespace IdentityService.Services
{
    public class GrpcIdentityService : GrpcIdentity.GrpcIdentityBase
    {
        private readonly IdentitySvcDbContext _dbContext;
        private readonly ILogger<GrpcIdentityService> _logger;
        public GrpcIdentityService(IdentitySvcDbContext dbContext, ILogger<GrpcIdentityService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override async Task<GetUsersByIdsResponse> GetUsersByIds(GetUsersByIdsRequest request, ServerCallContext context)
        {
            var response = new GetUsersByIdsResponse();
            var users = await _dbContext.Users.Where(u => request.UserIds.Contains(u.Id))
                .Include(u => u.Image).ToListAsync();
            foreach (var user in users)
            {
                var userInfo = new UserInfo
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    UserEmail = user.Email,
                    IsAdmin = user.IsAdmin,
                };

                if (user.Image?.Url != null)
                {
                    userInfo.ImageUrl = user.Image.Url;
                }

                response.Users.Add(userInfo);
            }
            return response;
        }

        public override async Task<GetAllUsersResponse> GetAllUsers(Empty request, ServerCallContext context)
        {
            var response = new GetAllUsersResponse();
            var users = await _dbContext.Users.ToListAsync();
            foreach (var user in users)
            {
                var userInfo = new UserInfo
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    UserEmail = user.Email,
                    IsAdmin = user.IsAdmin,
                };

                if (user.Image?.Url != null)
                {
                    userInfo.ImageUrl = user.Image.Url;
                }

                response.Users.Add(userInfo);
            }
            return response;
        }
    }
}