using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chame.ContentLoaders;
using Chame.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Chame.Internal
{
    internal class ChameService : IChameService
    {
        public async Task<byte[]> GetContentAsync(string extension, string filter, HttpContext httpContext)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            ChameUtility utils = GetUtils(httpContext);

            ITheme theme = utils.GetTheme(httpContext);

            return await utils.GetContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<byte[]> GetContentAsync(string extension, string filter, ITheme theme, HttpContext httpContext)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            ChameUtility utils = GetUtils(httpContext);

            return await utils.GetContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, HttpContext httpContext)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            ChameUtility utils = GetUtils(httpContext);

            ITheme theme = utils.GetTheme(httpContext);

            return await utils.LoadContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, ITheme theme, HttpContext httpContext)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            ChameUtility utils = GetUtils(httpContext);

            return await utils.LoadContentAsync(extension, filter, theme, httpContext);
        }

        private static ChameUtility GetUtils(HttpContext httpContext)
        {
            return httpContext.RequestServices.GetRequiredService<ChameUtility>();
        }
    }
}