using System;
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

            if (IsEnabled(support))
            {
                if (_mem.TryGetValue(KeyFor<T>(context), out T item))
                {
                    return item;
                }
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

            if (IsEnabled(support))
            {
                _mem.Set(KeyFor<T>(context), item, support.AbsoluteExpirationRelativeToNow);
            }
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
            var key = context.Filter == null ?
                $"__Chame.Caching.ContentCache.Key(type:'{typeof(T).FullName}';content:'{context.ContentInfo.Extension}';filter:'';theme:'{context.Theme.GetName()}';)" :
                $"__Chame.Caching.ContentCache.Key(type:'{typeof(T).FullName}';content:'{context.ContentInfo.Extension}';filter:'{context.Filter}';theme:'{context.Theme.GetName()}';)";
            return key;
        }
    }
}
