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
        private readonly IConfiguration _config;
        public GrpcIdentityClient(IConfiguration config)
        {
            _config = config;
        }
        public async Task<List<UserInfo>> GetUsersByIds(List<string> userIds)
        {
            var channel = GrpcChannel.ForAddress(_config["GrpcIdentity"] ?? throw new InvalidOperationException("GrpcIdentity address is not configured"));
            var client = new GrpcIdentity.GrpcIdentityClient(channel);
            var request = new GetUsersByIdsRequest();
            request.UserIds.AddRange(userIds);
            var response = await client.GetUsersByIdsAsync(request);
            return response.Users.ToList();
        }
    }
}