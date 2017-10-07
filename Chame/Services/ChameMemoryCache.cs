using System;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Chame.Services
{ 
    internal sealed class ChameMemoryCache
    {
        private readonly IMemoryCache _memoryCache;

        public ChameMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T Get<T>(ChameContext context)
        {
            string key = GetKey<T>(context);
            if (_memoryCache.TryGetValue(key, out T item))
            {
                return item;
            }
            return default(T);
        }

        public void Set<T>(T item, TimeSpan absoluteExpirationRelativeToNow, ChameContext context)
        {
            string key = GetKey<T>(context);
            _memoryCache.Set(key, item, absoluteExpirationRelativeToNow);
        }

        private static string GetKey<T>(ChameContext context)
        {
            var buffer = new StringBuilder(256);
            buffer.Append(typeof(ChameMemoryCache).FullName);
            buffer.Append("{type:'");
            buffer.Append(typeof(T).FullName);
            buffer.Append("';category:'");
            buffer.Append(context.Category);
            buffer.Append("';filter:");
            buffer.Append("'");
            if (context.Filter != null)
            {
                buffer.Append(context.Filter);
            }
            buffer.Append("';theme:");
            buffer.Append("'");
            if (context.Theme != null)
            {
                buffer.Append(context.Theme);
            }
            buffer.Append("';}");
            return buffer.ToString();
        }
    }
}
