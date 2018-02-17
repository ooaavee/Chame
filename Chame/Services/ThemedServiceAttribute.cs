using Chame.Themes;
using System;

namespace Chame.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ThemedServiceAttribute : Attribute
    {        
        public ThemedServiceAttribute(string themeName)
        {
            if (!Theme.IsValidThemeName(themeName))
            {
                throw new ArgumentException("Not a valid theme name.", nameof(themeName));
            }

            ThemeName = themeName;
        }

        public string ThemeName { get; }
    }
}
