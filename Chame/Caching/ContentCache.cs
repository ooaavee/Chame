using System;
using System.Text;
using Chame.ContentLoaders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace Chame.Caching
{
    public class ContentCache
    {
        private readonly IMemoryCache _mem;
        private readonly IHostingEnvironment _env;

        public ContentCache(IMemoryCache mem, IHostingEnvironment env)
        {
            if (mem == null)
            {
                throw new ArgumentNullException(nameof(mem));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            _mem = mem;
            _env = env;
        }

        public T Get<T>(CachingSupport support, ContentLoadingContext context) where T : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!IsEnabled(support))
            {
                return null;
            }

            string key = KeyFor<T>(context);

            if (_mem.TryGetValue(key, out T item))
            {
                return item;
            }

            return null;
        }

        public void Set<T>(T item, CachingSupport support, ContentLoadingContext context) where T : class
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }            

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!IsEnabled(support))
            {
                return;
            }

            string key = KeyFor<T>(context);

            _mem.Set(key, item, support.AbsoluteExpirationRelativeToNow);
        }

        private bool IsEnabled(CachingSupport support)
        {
            switch (support.Mode)
            {
                case CachingModes.Disabled:
                    return false;
                case CachingModes.Enabled:
                    return true;
                case CachingModes.EnabledButDisabledOnDev:
                    if (_env.IsDevelopment())
                    {
                        return false;
                    }
                    return true;
                default:
                    return false;
            }
        }

        private static string KeyFor<T>(ContentLoadingContext context)
        {
            var s = new StringBuilder(256);
            s.Append(typeof(ContentCache).FullName);
            s.Append("{type:'");
            s.Append(typeof(T).FullName);
            s.Append("';content:'");
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
