using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using IdentityService.Grpc;

namespace NotificationService.Services
{
    public class GrpcIdentityClient
    {
        private readonly GrpcIdentity.GrpcIdentityClient _client;
        public GrpcIdentityClient(GrpcIdentity.GrpcIdentityClient client)
        {
            _client = client;
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
    }
}