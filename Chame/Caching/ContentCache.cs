using System;
using System.Text;
using Chame.ContentLoaders;
using Microsoft.Extensions.Caching.Memory;

namespace Chame.Caching
{ 
    public class ContentCache
    {
        private readonly IMemoryCache _mem;

        public ContentCache(IMemoryCache mem)
        {
            if (mem == null)
            {
                throw new ArgumentNullException(nameof(mem));
            }

            _mem = mem;
        }

        public T Get<T>(ContentLoadingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string key = KeyFor<T>(context);

            if (_mem.TryGetValue(key, out T item))
            {
                return item;
            }

            return default(T);
        }

        public void Set<T>(T item, TimeSpan absoluteExpirationRelativeToNow, ContentLoadingContext context)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (absoluteExpirationRelativeToNow == null)
            {
                throw new ArgumentNullException(nameof(absoluteExpirationRelativeToNow));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string key = KeyFor<T>(context);

            _mem.Set(key, item, absoluteExpirationRelativeToNow);
        }

        private static string KeyFor<T>(ContentLoadingContext context)
        {
            var s = new StringBuilder(256);
            s.Append(typeof(ContentCache).FullName);
            s.Append("{type:'");
            s.Append(typeof(T).FullName);
            s.Append("';category:'");
            s.Append(context.ContentInfo.Extension);
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
