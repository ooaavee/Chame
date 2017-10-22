using System;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Chame.Services
{ 
    public class ContentCache
    {
        private readonly IMemoryCache _mem;

        public ContentCache(IMemoryCache mem)
        {
            _mem = mem;
        }

        public T Get<T>(ContentLoadingContext context)
        {
            string key = GetKey<T>(context);

            if (_mem.TryGetValue(key, out T item))
            {
                return item;
            }

            return default(T);
        }

        public void Set<T>(T item, TimeSpan absoluteExpirationRelativeToNow, ContentLoadingContext context)
        {
            string key = GetKey<T>(context);

            _mem.Set(key, item, absoluteExpirationRelativeToNow);
        }

        private static string GetKey<T>(ContentLoadingContext context)
        {
            var s = new StringBuilder(256);
            s.Append(typeof(ContentCache).FullName);
            s.Append("{type:'");
            s.Append(typeof(T).FullName);
            s.Append("';category:'");
            s.Append(context.Category);
            s.Append("';filter:");
            s.Append("'");
            if (context.Filter != null)
            {
                s.Append(context.Filter);
            }
            s.Append("';theme:");
            s.Append("'");
            if (context.Theme != null)
            {
                s.Append(context.Theme);
            }
            s.Append("';}");
            return s.ToString();
        }
    }
}
