using Chame.ContentLoaders;
using System;
using System.Threading.Tasks;

namespace WebSite
{
    public class DemoContentNotFoundCallback : IContentNotFoundCallback
    {
        private static readonly byte[] NoData = new byte[0];

        public Task<byte[]> GetDefaultContentAsync(ContentLoadingContext context)
        {
            byte[] data = null;

            if (context.ContentInfo.Extension.Equals("js", StringComparison.InvariantCultureIgnoreCase) ||
                context.ContentInfo.Extension.Equals("css", StringComparison.InvariantCultureIgnoreCase))
            {
                data = NoData;
            }

            return Task.FromResult(data);
        }
    }
}
