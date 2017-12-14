using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chame.ContentLoaders
{
    public static class ContentLoadingContextFactory
    {
        public static ContentLoadingContext Create(HttpContext http)
        {
            ILoggerFactory loggerFactory = http.RequestServices.GetRequiredService<ILoggerFactory>();
            ILogger _logger = loggerFactory.CreateLogger(typeof(ContentLoadingContextFactory));





            return null;
        }
    }
}
