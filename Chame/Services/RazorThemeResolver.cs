using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Chame.Services
{
    internal sealed class RazorThemeResolver : IChameRazorThemeResolver
    {
        private readonly ChameRazorThemeResolverOptions _options;
        private readonly ILogger<RazorThemeResolver> _logger;

        public RazorThemeResolver(IOptions<ChameRazorThemeResolverOptions> options, ILogger<RazorThemeResolver> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Resolves a theme that should be used.
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>theme name</returns>
        public string ResolveTheme(ChameRazorThemeResolveContext context)
        {
            string theme;

            if (_options.ResolveTheme != null)
            {
                try
                {
                    theme = _options.ResolveTheme(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ResolveTheme threw an unhandled exception.");
                    throw;
                }
            }
            else
            {
                theme = _options.DefaultTheme;
            }

            return theme;
        }

    }
}
