using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Chame.Loaders.FileSystem
{
    internal class Cache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly FileSystemContentLoaderOptions _options;

        public Cache(IMemoryCache memoryCache, IOptions<FileSystemContentLoaderOptions> options)
        {
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        public T Get<T>(CacheBlock block, ChameContext context)
        {
            var key = GetKey(block, context);
            T item;
            if (_memoryCache.TryGetValue(key, out item))
            {
                return item;
            }
            return default(T);
        }

        public void Set<T>(T item, CacheBlock block, ChameContext context)
        {
            var key = GetKey(block, context);
            _memoryCache.Set(key, item, _options.CacheAbsoluteExpirationRelativeToNow);
        }

        private static string GetKey(CacheBlock block, ChameContext context)
        {
            var buf = new StringBuilder(256);
            buf.Append(typeof(Cache).FullName);
            buf.Append(";");
            buf.Append("block:");
            buf.Append("'" + block + "'");
            buf.Append(";");
            buf.Append("category:");
            buf.Append("'" + context.Category + "'");
            buf.Append(";");
            buf.Append("filter:");
            if (string.IsNullOrEmpty(context.Filter))
            {
                buf.Append("none");
            }
            else
            {
                buf.Append("'" + context.Filter + "'");
            }
            buf.Append(";");
            buf.Append("theme:");
            if (string.IsNullOrEmpty(context.Theme))
            {
                buf.Append("none");
            }
            else
            {
                buf.Append("'" + context.Theme + "'");
            }
            buf.Append(";");
            return buf.ToString();
        }

        public enum CacheBlock
        {
            ThemeBundle,
            FileContent
        }

    }
}
