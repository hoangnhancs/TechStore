using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using IdentityService.Grpc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using NotificationService.Persistence;

namespace NotificationService.Services
{
    public class GrpcIdentityClient
    {
        private readonly GrpcIdentity.GrpcIdentityClient _client;
        private readonly INotificationUnitOfWork _unitOfWork;
        public GrpcIdentityClient(GrpcIdentity.GrpcIdentityClient client, INotificationUnitOfWork unitOfWork)
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

        public async Task<List<UserInfo>> GetAllUsers()
        {
            var response = await _client.GetAllUsersAsync(new Empty());
            return response.Users.ToList();
        }

        public async Task<UserInfo?> GetSystemUser()
        {
            return await _client.GetSystemUserAsync(new Empty());
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

            var systemUser = await GetSystemUser();

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
                existingUser.PhoneNumber = sourceUser.PhoneNumber;
                existingUser.UserEmail = sourceUser.UserEmail;
                existingUser.UpdatedAt = DateTime.UtcNow;
            }

            var newUsers = users.Values
                .Where(u => !existingUserIds.Contains(u.UserId))
                .Select(u => new UserInformation
                {
                    UserId = u.UserId,
                    DisplayName = u.DisplayName,
                    ImageUrl = u.ImageUrl,
                    PhoneNumber = u.PhoneNumber,
                    UserEmail = u.UserEmail,
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