using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shared.Web.Helper.Interface;

namespace Shared.Web.Helper
{
    public class HttpContextAccessorHelper : IHttpContextAccessorHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HttpContextAccessorHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? context?.Connection.RemoteIpAddress?.ToString()
                ?? string.Empty;
        }
    }
}