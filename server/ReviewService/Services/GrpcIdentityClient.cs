using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using IdentityService.Grpc;
using Microsoft.EntityFrameworkCore;
using ReviewService.Entities;
using ReviewService.Persistence;

namespace ReviewService.Services
{
    public class GrpcIdentityClient
    {
        private readonly GrpcIdentity.GrpcIdentityClient _client;
        private readonly IReviewUnitOfWork _unitOfWork;
        public GrpcIdentityClient(GrpcIdentity.GrpcIdentityClient client, IReviewUnitOfWork unitOfWork)
        {
            _client = client;
            _unitOfWork = unitOfWork;
        }
        public async Task<List<UserInfo>> GetUsersByIds(List<string> userIds)
        {
            var request = new GetUsersByIdsRequest();
            request.UserIds.AddRange(userIds);
            var response = await _client.GetUsersByIdsAsync(request);
            return response.Users.ToList();
        }

        public async Task<List<UserInfo>> GetUserByLastUpdated(DateTime lastUpdated)
        {
            var request = new GetUserByLastUpdatedRequest
            {
                LastUpdated = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTime(lastUpdated.ToUniversalTime())
            };

            var response = await _client.GetUserByLastUpdatedAsync(request);

            return response.Users.ToList();
        }

        public async Task SyncUserInformation()
        {
            // var lastUpdated =
            //     (await _unitOfWork.UserInformationRepository
            //         .GetAll()
            //         .OrderByDescending(u => u.UpdatedAt)
            //         .Select(u => (DateTime?)u.UpdatedAt)
            //         .FirstOrDefaultAsync())
            //     ?? DateTime.MinValue;
            var lastUpdated = DateTime.MinValue; // Sync all users 

            var users = (await GetUserByLastUpdated(lastUpdated))
                .ToDictionary(u => u.UserId);

            if (users.Count == 0)
            {
                return;
            }

            var existingUsers = await _unitOfWork.UserInformationRepository
                .GetAll()
                .Where(u => users.Keys.Contains(u.UserId))
                .ToListAsync();

            var existingUserIds = existingUsers
                .Select(u => u.UserId)
                .ToHashSet();

            foreach (var existingUser in existingUsers)
            {
                var sourceUser = users[existingUser.UserId];

                existingUser.DisplayName = sourceUser.DisplayName;
                existingUser.ImageUrl = sourceUser.ImageUrl;
                existingUser.UpdatedAt = DateTime.UtcNow;
                existingUser.PhoneNumber = sourceUser.PhoneNumber;
            }

            var newUsers = users.Values
                .Where(u => !existingUserIds.Contains(u.UserId))
                .Select(u => new UserInformation
                {
                    UserId = u.UserId,
                    DisplayName = u.DisplayName,
                    ImageUrl = u.ImageUrl,
                    PhoneNumber = u.PhoneNumber,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (newUsers.Count > 0)
            {
                await _unitOfWork.UserInformationRepository.AddRangeAsync(newUsers);
            }

            await _unitOfWork.CommitAsync();
        }
    }
}