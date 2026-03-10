using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Web.Helper.Interface
{
    public interface IHttpContextAccessorHelper
    {
        string GetClientIp();
    }
}