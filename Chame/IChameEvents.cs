using System;
using Chame.Extensions;

namespace Chame
{
    public interface IChameEvents
    {
        Func<ThemeResolverEventArgs, string> ThemeResolver { get; set; }

        Action<IContentLoader[]> ContentLoaderSorter { get; set; }
    }
}
