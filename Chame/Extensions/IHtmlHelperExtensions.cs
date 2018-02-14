using Chame;
using Chame.Razor;
using Microsoft.AspNetCore.Html;
using System;
using Chame.Themes;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class IHtmlHelperExtensions
    {
        ////public static IHtmlContent Partial(this IHtmlHelper htmlHelper, string partialViewName, ITheme theme)
        ////{
        ////    if (htmlHelper == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(htmlHelper));
        ////    }

        ////    if (partialViewName == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(partialViewName));
        ////    }

        ////    if (theme == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(theme));
        ////    }

        ////    ThemedViewLocationExpander.UseThemeWithHttpContext(theme, htmlHelper.ViewContext.HttpContext);

        ////    return htmlHelper.Partial(partialViewName);
        ////}
    }
}
