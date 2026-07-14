using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;

namespace GatewayService.Middleware
{
    /// <summary>
    /// Proactively refreshes an expiring access_token before forwarding the request downstream,
    /// so the client never sees a 401 caused purely by token expiry.
    /// </summary>
    public class TokenRefreshMiddleware
    {
        private const string AccessTokenCookie = "access_token";
        private const string RefreshTokenCookie = "refresh_token";
        private static readonly PathString RefreshEndpointPath = "/account/refreshToken";
        private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan ResultCacheDuration = TimeSpan.FromSeconds(10);

        // Dedupe concurrent refresh attempts that share the same refresh_token,
        // tranh truong hop rotate refresh_token 2 lan cung luc lam request con lai bi 401 oan.
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        private readonly RequestDelegate _next;

        public TokenRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<TokenRefreshMiddleware> logger)
        {
            if (!context.Request.Path.StartsWithSegments(RefreshEndpointPath))
            {
                var accessToken = context.Request.Cookies[AccessTokenCookie];
                var refreshToken = context.Request.Cookies[RefreshTokenCookie];

                if (!string.IsNullOrEmpty(accessToken)
                    && !string.IsNullOrEmpty(refreshToken)
                    && IsExpiredOrExpiringSoon(accessToken, logger))
                {
                    var newTokens = await RefreshTokensAsync(refreshToken, httpClientFactory, cache, logger);
                    if (newTokens != null)
                    {
                        ApplyToOutgoingRequest(context, newTokens);
                        ApplyToResponse(context, newTokens);
                    }
                    // Refresh that bai: cu de request di tiep voi token cu, downstream se tra 401 nhu binh thuong.
                }
            }

            await _next(context);
        }

        private static bool IsExpiredOrExpiringSoon(string accessToken, ILogger logger)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                return jwt.ValidTo <= DateTime.UtcNow.Add(ExpiryBuffer);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Khong doc duoc access_token, coi nhu het han va thu refresh");
                return true;
            }
        }

        private static async Task<RefreshResult?> RefreshTokensAsync(
            string refreshToken,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger logger)
        {
            if (cache.TryGetValue(refreshToken, out RefreshResult? cached))
                return cached;

            var gate = _locks.GetOrAdd(refreshToken, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                if (cache.TryGetValue(refreshToken, out cached))
                    return cached;

                var client = httpClientFactory.CreateClient("IdentityService");
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/refreshToken");
                request.Headers.Add("Cookie", $"{RefreshTokenCookie}={refreshToken}");

                using var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Refresh token khong hop le, status {Status}", response.StatusCode);
                    return null;
                }

                var newAccess = ExtractCookie(response, AccessTokenCookie);
                var newRefresh = ExtractCookie(response, RefreshTokenCookie);
                if (newAccess is null || newRefresh is null) return null;

                var result = new RefreshResult(newAccess.Value.Value, newAccess.Value.Expires, newRefresh.Value.Value, newRefresh.Value.Expires);
                cache.Set(refreshToken, result, ResultCacheDuration);
                return result;
            }
            finally
            {
                gate.Release();
                _locks.TryRemove(refreshToken, out _);
            }
        }

        private static (string Value, DateTimeOffset? Expires)? ExtractCookie(HttpResponseMessage response, string name)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                return null;

            foreach (var header in SetCookieHeaderValue.ParseList(setCookieHeaders.ToList()))
            {
                if (header.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return (header.Value.Value ?? string.Empty, header.Expires);
            }
            return null;
        }

        private static void ApplyToOutgoingRequest(HttpContext context, RefreshResult tokens)
        {
            var cookies = context.Request.Headers["Cookie"]
                .ToString()
                .Split("; ", StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0], p => p[1]);

            cookies[AccessTokenCookie] = tokens.AccessToken;
            cookies[RefreshTokenCookie] = tokens.RefreshToken;

            context.Request.Headers["Cookie"] = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
        }

        private static void ApplyToResponse(HttpContext context, RefreshResult tokens)
        {
            context.Response.Cookies.Append(AccessTokenCookie, tokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = tokens.AccessTokenExpires
            });
            context.Response.Cookies.Append(RefreshTokenCookie, tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = tokens.RefreshTokenExpires
            });
        }

        private sealed record RefreshResult(string AccessToken, DateTimeOffset? AccessTokenExpires, string RefreshToken, DateTimeOffset? RefreshTokenExpires);
    }
}
