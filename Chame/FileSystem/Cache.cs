using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Chame.FileSystem.Services
{
    internal sealed class Cache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ContentLoaderOptions _options;

        public Cache(IMemoryCache memoryCache, IOptions<ContentLoaderOptions> options)
        {
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        public T Get<T>(Block block, ChameContext context)
        {
            var key = GetKey(block, context);
            T item;
            if (_memoryCache.TryGetValue(key, out item))
            {
                return item;
            }
            return default(T);
        }

        public void Set<T>(T item, Block block, ChameContext context)
        {
            var key = GetKey(block, context);
            _memoryCache.Set(key, item, _options.CacheAbsoluteExpirationRelativeToNow);
        }

        private static string GetKey(Block block, ChameContext context)
        {
            var s = new StringBuilder();
            s.Append(typeof(Cache).FullName);
            s.Append(";");
            s.Append("block:");
            s.Append("'" + block + "'");
            s.Append(";");
            s.Append("category:");
            s.Append("'" + context.Category + "'");
            s.Append(";");
            s.Append("filter:");
            if (string.IsNullOrEmpty(context.Filter))
            {
                s.Append("none");
            }
            else
            {
                s.Append("'" + context.Filter + "'");
            }
            s.Append(";");
            s.Append("theme:");
            if (string.IsNullOrEmpty(context.Theme))
            {
                s.Append("none");
            }
            else
            {
                s.Append("'" + context.Theme + "'");
            }
            s.Append(";");
            return s.ToString();
        }

        public enum Block
        {
            ThemeBundle,
            BundleContent
        }

    }
}
