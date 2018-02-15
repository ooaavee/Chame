using System;
using System.Collections.Generic;
using System.Text;
using Chame.Themes;

namespace Chame.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ThemedServiceAttribute : Attribute
    {
        public ThemedServiceAttribute(string themeName)
        {
            Theme = new Theme(themeName);
        }

        public ITheme Theme { get; }
    }
}
