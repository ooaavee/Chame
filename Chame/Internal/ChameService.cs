using Chame.Services;
using Chame.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chame.Internal
{
    internal class ChameService : IChameService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ChameUtility _utils;

        public ChameService(IServiceProvider serviceProvider, ChameUtility utils)
        {
            _serviceProvider = serviceProvider;
            _utils = utils;
        }

        public async Task<byte[]> LoadContentAsync(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            HttpContext httpContext = AccessHttpContext();
            ITheme theme = _utils.GetTheme(httpContext);
            string extension = GetExtension(fileName);
            return await _utils.LoadContentAsync(extension, fileName, theme, httpContext);
        }

        public async Task<byte[]> LoadContentAsync(string fileName, ITheme theme)
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
            return await _utils.LoadContentAsync(extension, fileName, theme, httpContext);
        }

        public async Task<byte[]> LoadContentAsync(string fileName, HttpContext httpContext)
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
            return await _utils.LoadContentAsync(extension, fileName, theme, httpContext);
        }

        public async Task<byte[]> LoadContentAsync(string fileName, HttpContext httpContext, ITheme theme)
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
            return await _utils.LoadContentAsync(extension, fileName, theme, httpContext);
        }

        public async Task<byte[]> LoadContentAsync(string extension, string filter, HttpContext httpContext)
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
            return await _utils.LoadContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<byte[]> LoadContentAsync(string extension, string filter)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            HttpContext httpContext = AccessHttpContext();
            ITheme theme = _utils.GetTheme(httpContext);
            return await _utils.LoadContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<byte[]> LoadContentAsync(string extension, string filter, HttpContext httpContext, ITheme theme)
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

            return await _utils.LoadContentAsync(extension, filter, theme, httpContext);
        }

        public async Task<byte[]> LoadContentAsync(string extension, string filter, ITheme theme)
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
            return await _utils.LoadContentAsync(extension, filter, theme, httpContext);
        }

        public T GetThemedService<T>()
        {
            HttpContext httpContext = AccessHttpContext();
            ITheme theme = _utils.GetTheme(httpContext);
            IEnumerable<T> services = GetServices<T>(httpContext);
            T result = FindService(services, theme);
            return result;
        }

        public T GetThemedService<T>(HttpContext httpContext, ITheme theme)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            IEnumerable<T> services = GetServices<T>(httpContext);
            T result = FindService(services, theme);
            return result;
        }

        public T GetThemedService<T>(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            ITheme theme = _utils.GetTheme(httpContext);
            IEnumerable<T> services = GetServices<T>(httpContext);
            T result = FindService(services, theme);
            return result;
        }

        public T GetThemedService<T>(ITheme theme)
        {
            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            HttpContext httpContext = AccessHttpContext();
            IEnumerable<T> services = GetServices<T>(httpContext);
            T result = FindService(services, theme);
            return result;
        }

        public IEnumerable<T> GetThemedServices<T>()
        {
            HttpContext httpContext = AccessHttpContext();
            ITheme theme = _utils.GetTheme(httpContext);
            IEnumerable<T> services = GetServices<T>(httpContext);
            IEnumerable<T> result = FindServices(services, theme);
            return result;
        }

        public IEnumerable<T> GetThemedServices<T>(HttpContext httpContext, ITheme theme)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            IEnumerable<T> services = GetServices<T>(httpContext);
            IEnumerable<T> result = FindServices(services, theme);
            return result;
        }

        public IEnumerable<T> GetThemedServices<T>(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            ITheme theme = _utils.GetTheme(httpContext);
            IEnumerable<T> services = GetServices<T>(httpContext);
            IEnumerable<T> result = FindServices(services, theme);
            return result;
        }

        public IEnumerable<T> GetThemedServices<T>(ITheme theme)
        {
            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            HttpContext httpContext = AccessHttpContext();
            IEnumerable<T> services = GetServices<T>(httpContext);
            IEnumerable<T> result = FindServices(services, theme);
            return result;
        }

        private IEnumerable<T> GetServices<T>(HttpContext httpContext)
        {
            IEnumerable<T> services = httpContext.RequestServices.GetServices<T>();
            return services;
        }

        private T FindService<T>(IEnumerable<T> services, ITheme theme)
        {
            IEnumerable<T> candidates = FindServices(services, theme);
            T result = candidates.LastOrDefault();
            return result;
        }

        private IEnumerable<T> FindServices<T>(IEnumerable<T> services, ITheme theme)
        {
            List<T> matches = new List<T>();
            List<T> fallbacks = new List<T>();

            foreach (T candidate in services)
            {
                ThemedServiceMatchResult match = GetMatch(candidate, theme);

                switch (match)
                {
                    case ThemedServiceMatchResult.Match:
                        matches.Add(candidate);
                        break;
                    case ThemedServiceMatchResult.Fallback:
                        fallbacks.Add(candidate);
                        break;
                }
            }

            if (matches.Any())
            {
                return matches;
            }

            return fallbacks;
        }

        private static ThemedServiceMatchResult GetMatch<T>(T candidate, ITheme theme)
        {
            Type type = candidate.GetType();

            ThemedServiceAttribute[] tmpThemed = (ThemedServiceAttribute[]) type.GetCustomAttributes(typeof(ThemedServiceAttribute), true);
            if (tmpThemed.Any())
            {
                ThemedServiceAttribute themed = tmpThemed.First();
                if (themed != null)
                {
                    if (theme.GetName().Equals(themed.ThemeName))
                    {
                        return ThemedServiceMatchResult.Match;
                    }
                }
            }

            FallbackServiceAttribute[] tmpFallback = (FallbackServiceAttribute[]) type.GetCustomAttributes(typeof(FallbackServiceAttribute), true);
            if (tmpFallback.Any())
            {
                return ThemedServiceMatchResult.Fallback;
            }

            return ThemedServiceMatchResult.NotMatch;
        }

        private HttpContext AccessHttpContext()
        {
            IHttpContextAccessor accessor = _serviceProvider.GetService<IHttpContextAccessor>();
            if (accessor == null)
            {
                throw new InvalidOperationException($"The method requires that {nameof(IHttpContextAccessor)} service is registered.");
            }
            return accessor.HttpContext;
        }

        private static string GetExtension(string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            if (!string.IsNullOrEmpty(file.Extension))
            {
                if (file.Extension.Length > 1)
                {
                    return file.Extension.Substring(1);
                }
            }
            throw new ArgumentException("Unable to parse file extension.", nameof(fileName));
        }

        private enum ThemedServiceMatchResult
        {
            Match,
            NotMatch,
            Fallback
        }

    }
}