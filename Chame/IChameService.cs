using Chame.Themes;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chame
{
    public interface IChameService
    {
        Task<byte[]> LoadContentAsync(string fileName);
        Task<byte[]> LoadContentAsync(string fileName, ITheme theme);
        Task<byte[]> LoadContentAsync(string fileName, HttpContext httpContext);
        Task<byte[]> LoadContentAsync(string fileName, HttpContext httpContext, ITheme theme);

        Task<byte[]> LoadContentAsync(string extension, string filter);
        Task<byte[]> LoadContentAsync(string extension, string filter, HttpContext httpContext);
        Task<byte[]> LoadContentAsync(string extension, string filter, HttpContext httpContext, ITheme theme);
        Task<byte[]> LoadContentAsync(string extension, string filter, ITheme theme);

        T GetThemedService<T>();
        T GetThemedService<T>(HttpContext httpContext, ITheme theme);
        T GetThemedService<T>(HttpContext httpContext);
        T GetThemedService<T>(ITheme theme);

        IEnumerable<T> GetThemedServices<T>();
        IEnumerable<T> GetThemedServices<T>(HttpContext httpContext, ITheme theme);
        IEnumerable<T> GetThemedServices<T>(HttpContext httpContext);
        IEnumerable<T> GetThemedServices<T>(ITheme theme);
    }
}