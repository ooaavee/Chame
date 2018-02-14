using System.Collections.Generic;
using System.Threading.Tasks;
using Chame.ContentLoaders;
using Chame.Themes;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public interface IChameService
    {
        // TODO: Tästä vielä overload, jossa yksi string parametri fileName, josta ripataan extension ja filename on samalla filter!
        Task<byte[]> GetContentAsync(string extension, string filter, HttpContext httpContext);
        Task<byte[]> GetContentAsync(string extension, string filter, ITheme theme, HttpContext httpContext);

        // TODO: Tästä vielä overload, jossa yksi string parametri fileName, josta ripataan extension ja filename on samalla filter!
        Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, HttpContext httpContext);
        Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, ITheme theme, HttpContext httpContext);
    }
}