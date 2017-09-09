using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public interface IChameContextFactory
    {
        bool TryCreateContext(HttpContext httpContext, out ChameContext context);
    }
}
