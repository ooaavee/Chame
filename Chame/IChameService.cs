using System.Collections.Generic;
using System.Threading.Tasks;
using Chame.ContentLoaders;
using Chame.Themes;
using Microsoft.AspNetCore.Http;

namespace Chame
{
    public interface IChameService
    {
        Task<byte[]> GetContentAsync(string fileName);
        Task<byte[]> GetContentAsync(string fileName, ITheme theme);
        Task<byte[]> GetContentAsync(string fileName, HttpContext httpContext);
        Task<byte[]> GetContentAsync(string fileName, HttpContext httpContext, ITheme theme);

        Task<byte[]> GetContentAsync(string extension, string filter);
        Task<byte[]> GetContentAsync(string extension, string filter, HttpContext httpContext);
        Task<byte[]> GetContentAsync(string extension, string filter, HttpContext httpContext, ITheme theme);
        Task<byte[]> GetContentAsync(string extension, string filter, ITheme theme);

        // TODO: Tästä vielä overload, jossa yksi string parametri fileName, josta ripataan extension ja filename on samalla filter!
        ////Task<IList<ContentLoaderResponse>> LoadContentAsync(string fileName, HttpContext httpContext);
        ////Task<IList<ContentLoaderResponse>> LoadContentAsync(string fileName, HttpContext httpContext, ITheme theme);
        ////Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, HttpContext httpContext);
        ////Task<IList<ContentLoaderResponse>> LoadContentAsync(string extension, string filter, HttpContext httpContext, ITheme theme);
    }
}