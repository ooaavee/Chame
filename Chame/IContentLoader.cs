using System;
using System.Threading.Tasks;

namespace Chame
{
    public interface IContentLoader
    {
        /// <summary>
        /// Content loader priority. 
        /// An execution order of content loaders are sorted by this property. 
        /// This is only meaningful if there are more than one content loaders!
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Loads content.
        /// </summary>
        /// <param name="context">A context object that tells you what was requested.</param>
        /// <returns>response</returns>
        Task<TextContent> LoadContentAsync(ContentLoadingContext context);
    }

    public interface IJsContentLoader : IContentLoader
    {
    }

    public interface ICssContentLoader : IContentLoader
    {
    }

    internal class FuncContentLoader : IContentLoader
    {
        private Func<ContentLoadingContext, Task<TextContent>> Func { get; }
        public int Priority { get; }

        public FuncContentLoader(Func<ContentLoadingContext, Task<TextContent>> func, int priority)
        {
            Func = func;
            Priority = priority;
        }

        public async Task<TextContent> LoadContentAsync(ContentLoadingContext context)
        {
            return await Func(context);
        }
    }


}
