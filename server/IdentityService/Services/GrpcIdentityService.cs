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
                response.Users.Add(MapToUserInfo(user));
            }
            return response;
        }

        public override async Task<GetAllUsersResponse> GetAllUsers(Empty request, ServerCallContext context)
        {
            var response = new GetAllUsersResponse();
            var users = await _dbContext.Users.ToListAsync();
            foreach (var user in users)
            {
                response.Users.Add(MapToUserInfo(user));
            }
            return response;
        }

        public override async Task<UserInfo> GetSystemUser(Empty request, ServerCallContext context)
        {
            var systemUser = await _dbContext.Users
                .Include(u => u.Image)
                .FirstOrDefaultAsync(u => u.UserName == "system");

            if (systemUser == null)
                throw new RpcException(new Status(StatusCode.NotFound, "System user not found"));

            return MapToUserInfo(systemUser);
        }

        public override async Task<GetUserByLastUpdatedResponse> GetUserByLastUpdated(GetUserByLastUpdatedRequest request, ServerCallContext context)
        {
            var response = new GetUserByLastUpdatedResponse();
            var users = await _dbContext.Users
                .Where(u => u.UpdatedAt > request.LastUpdated.ToDateTime())
                .Include(u => u.Image)
                .ToListAsync();
            foreach (var user in users)
            {
                response.Users.Add(MapToUserInfo(user));
            }
            return response;
        }

        private static UserInfo MapToUserInfo(Entities.User user)
        {
            var info = new UserInfo
            {
                UserId = user.Id ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                UserEmail = user.Email ?? string.Empty,
                IsAdmin = user.IsAdmin,
                DisplayName = user.DisplayName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };
            if (user.Image?.Url != null)
                info.ImageUrl = user.Image.Url;
            return info;
        }
    }
}
