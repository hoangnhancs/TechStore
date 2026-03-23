using System;
using System.Text.Json;
using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Core.EF.Application;

namespace IdentityService.Services.Address;

public class CreateVirtualAddresses
{
    public class ProvinceResponse
    {
        public int code { get; set; }
        public string? message { get; set; }
        public List<ProvinceData> data { get; set; } = new();
    }
    public class DistrictResponse
    {
        public int code { get; set; }
        public string? message { get; set; }
        public List<DistrictData> data { get; set; } = new();
    }
    public class WardResponse
    {
        public int code { get; set; }
        public string? message { get; set; }
        public List<WardData> data { get; set; } = new();
    }

    public class ProvinceData
    {
        public int ProvinceID { get; set; }
        public string? ProvinceName { get; set; }
        public string? Code { get; set; }
    }
    public class DistrictData
    {
        public int DistrictID { get; set; }
        public string? DistrictName { get; set; }
        public string? Code { get; set; }
    }
    public class WardData
    {
        public string? WardCode { get; set; }
        public string? WardName { get; set; }
        public string? Code { get; set; }
    }

    public class CreateVirtualAddressesCommand : IRequest<AppResult<Unit>>
    {

    }
    public class CreateVirtualAddressesHandler : IRequestHandler<CreateVirtualAddressesCommand, AppResult<Unit>>
    {
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IdentitySvcDbContext _dbContext;
        public CreateVirtualAddressesHandler(IIdentityUnitOfWork unitOfWork, IConfiguration configuration, IdentitySvcDbContext dbContext)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }
        public async Task<AppResult<Unit>> Handle(CreateVirtualAddressesCommand request, CancellationToken cancellationToken)
        {
            if (_dbContext.Addresses.Any()) return AppResult<Unit>.Success(Unit.Value);
            Random random = new Random();
            var token = _configuration["GHN:ApiToken"];
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Token", token);

            var provinceResponse = await client.GetAsync($"https://online-gateway.ghn.vn/shiip/public-api/master-data/province");
            if (!provinceResponse.IsSuccessStatusCode)
                return AppResult<Unit>.Failure($"Failed to fetch provinces.", (int)provinceResponse.StatusCode);
            var provinceContent = await provinceResponse.Content.ReadAsStringAsync();
            var provinceJsonResult = JsonSerializer.Deserialize<ProvinceResponse>(provinceContent);
            var provinces = provinceJsonResult?.data;
            if (provinceJsonResult == null) return AppResult<Unit>.Failure($"Failed to fetch provinces.", 404);
            if (provinces == null) return AppResult<Unit>.Failure($"Failed to fetch provinces.", 404);

            var users = await _dbContext.Users.ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                var randomProvince = provinces[random.Next(provinces.Count)];
                var districtResponse = await client.GetAsync($"https://online-gateway.ghn.vn/shiip/public-api/master-data/district?province_id={randomProvince.ProvinceID}");
                if (!districtResponse.IsSuccessStatusCode)
                    return AppResult<Unit>.Failure($"Failed to fetch districts.", (int)districtResponse.StatusCode);
                var districtContent = await districtResponse.Content.ReadAsStringAsync();
                var districtJsonResult = JsonSerializer.Deserialize<DistrictResponse>(districtContent);
                var districts = districtJsonResult?.data;
                if (districtJsonResult == null) return AppResult<Unit>.Failure($"Failed to fetch address.", 404);
                if (districts == null) return AppResult<Unit>.Failure($"Failed to fetch address.", 404);

                var randomDistrict = districts[random.Next(districts.Count)];
                var wardResponse = await client.GetAsync($"https://online-gateway.ghn.vn/shiip/public-api/master-data/ward?district_id={randomDistrict.DistrictID}");
                if (!wardResponse.IsSuccessStatusCode)
                    return AppResult<Unit>.Failure($"Failed to fetch wards.", (int)wardResponse.StatusCode);
                var wardContent = await wardResponse.Content.ReadAsStringAsync();
                var wardJsonResult = JsonSerializer.Deserialize<WardResponse>(wardContent);
                var wards = wardJsonResult?.data;
                if (wardJsonResult == null) return AppResult<Unit>.Failure($"Failed to fetch address.", 404);
                if (wards == null) return AppResult<Unit>.Failure($"Failed to fetch address.", 404);
                var randomWard = wards[random.Next(wards.Count)];

                var address = new Entities.Address
                {
                    UserId = user.Id,
                    FullName = user.DisplayName ?? throw new Exception("User name is null"),
                    Province = randomProvince.ProvinceName ?? throw new Exception("Province name is null"),
                    District = randomDistrict.DistrictName ?? throw new Exception("District name is null"),
                    Ward = randomWard.WardName ?? throw new Exception("Ward name is null"),
                    DetailAddress = "123/45 test " + user.DisplayName?.ToLower(),
                    PhoneNumber = "01234566789",
                    IsDefault = true,
                };

                _dbContext.Addresses.Add(address);
            }
            var result = await _unitOfWork.CommitAsync(cancellationToken);
            if (!result) return AppResult<Unit>.Failure("Problem when create address", 400);
            return AppResult<Unit>.Success(Unit.Value);
        }
    }
}
