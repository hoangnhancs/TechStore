using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using IdentityService.Grpc;

namespace ReviewService.Services
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
    }
}