using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Chame.ContentLoaders;
using Chame.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chame.Internal
{
    internal class ChameService : IChameService
    {
        private readonly IServiceProvider _services;
        private readonly ChameUtility _utils;
        private readonly ILogger<ChameService> _logger;

        public ChameService(IServiceProvider services, ChameUtility utils, ILogger<ChameService> logger)
        {
            _services = services;
            _utils = utils;
            _logger = logger;
        }

        public async Task<byte[]> GetContentAsync(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            HttpContext httpContext = AccessHttpContext();
            ITheme theme = _utils.GetTheme(httpContext);
            string extension = GetExtension(fileName);
            return await _utils.GetContentAsync(extension, fileName, theme, httpContext);
        }

        public async Task<byte[]> GetContentAsync(string fileName, ITheme theme)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            HttpContext httpContext = AccessHttpContext();
            string extension = GetExtension(fileName);
            return await _utils.GetContentAsync(extension, fileName, theme, httpContext);
        }

        public async Task<byte[]> GetContentAsync(string fileName, HttpContext httpContext)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            ITheme theme = _utils.GetTheme(httpContext);
            string extension = GetExtension(fileName);
            return await _utils.GetContentAsync(extension, fileName, theme, httpContext);
        }

        public async Task<byte[]> GetContentAsync(string fileName, HttpContext httpContext, ITheme theme)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            string extension = GetExtension(fileName);
            return await _utils.GetContentAsync(extension, fileName, theme, httpContext);
        }

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

            ITheme theme = _utils.GetTheme(httpContext);
            return await _utils.GetContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<byte[]> GetContentAsync(string extension, string filter)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            HttpContext httpContext = AccessHttpContext();
            ITheme theme = _utils.GetTheme(httpContext);
            return await _utils.GetContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<byte[]> GetContentAsync(string extension, string filter, HttpContext httpContext, ITheme theme)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            return await _utils.GetContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<byte[]> GetContentAsync(string extension, string filter, ITheme theme)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            HttpContext httpContext = AccessHttpContext();
            return await _utils.GetContentAsync(extension, filter, theme, httpContext);
        }



        public T GetService<T>()
        {
            throw new NotImplementedException();
        }
        public T GetService<T>(HttpContext httpContext, ITheme theme)
        {
            throw new NotImplementedException();
        }
        public T GetService<T>(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }
        public T GetService<T>(ITheme theme)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<T> GetServices<T>()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<T> GetServices<T>(HttpContext httpContext, ITheme theme)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<T> GetServices<T>(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<T> GetServices<T>(ITheme theme)
        {
            throw new NotImplementedException();
        }



        ////public async Task<IList<ContentLoaderResponse>> LoadContentAsync(string fileName, HttpContext httpContext)
        ////{
        ////    if (fileName == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(fileName));
        ////    }

        ////    if (httpContext == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(httpContext));
        ////    }

        ////    ITheme theme = _utils.GetTheme(httpContext);
        ////    string extension = GetExtension(fileName);
        ////    return await _utils.LoadContentAsync(extension, fileName, httpContext, theme);
        ////}

        ////public async Task<IList<ContentLoaderResponse>> LoadContentAsync(string fileName, HttpContext httpContext, ITheme theme)
        ////{
        ////    if (fileName == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(fileName));
        ////    }

        ////    if (httpContext == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(httpContext));
        ////    }

        ////    string extension = GetExtension(fileName);
        ////    return await _utils.LoadContentAsync(extension, fileName, httpContext, theme);
        ////}

        ////public async Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, HttpContext httpContext)
        ////{
        ////    if (extension == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(extension));
        ////    }

        ////    if (httpContext == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(httpContext));
        ////    }

        ////    ITheme theme = _utils.GetTheme(httpContext);
        ////    return await _utils.LoadContentAsync(extension, filter, httpContext, theme);
        ////}

        ////public async Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, HttpContext httpContext, ITheme theme)
        ////{
        ////    if (extension == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(extension));
        ////    }

        ////    if (httpContext == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(httpContext));
        ////    }

        ////    if (theme == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(theme));
        ////    }

        ////    return await _utils.LoadContentAsync(extension, filter, httpContext, theme);
        ////}



        private HttpContext AccessHttpContext()
        {
            IHttpContextAccessor accessor = _services.GetService<IHttpContextAccessor>();
            if (accessor == null)
            {
                throw new InvalidOperationException($"This method requires that '{nameof(IHttpContextAccessor)}' service is registered.");
            }
            return accessor.HttpContext;
        }

        private static string GetExtension(string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            string extension = file.Extension;
            if (!string.IsNullOrEmpty(extension))
            {
                if (extension.Length > 1)
                {
                    return extension.Substring(1);
                }
            }
            throw new ArgumentException("Unable to parse extension.", nameof(fileName));
        }

    }
}