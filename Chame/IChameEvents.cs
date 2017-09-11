using System;
using Chame.Extensions;
using Chame.Loaders;

namespace Chame
{
    public interface IChameEvents
    {
        Func<ThemeResolverEventArgs, string> ThemeResolver { get; set; }

        Action<IContentLoader[]> ContentLoaderSorter { get; set; }
    }

}
