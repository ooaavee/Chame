using System;
using System.IO;
using System.Linq;

namespace Chame.Themes
{
    /// <summary>
    /// Theme implementation
    /// </summary>
    public class Theme : ITheme
    {
        private static readonly char[] IllegalChars = Path.GetInvalidFileNameChars();

        private readonly string _name;

        public Theme(string name)
        {
            if (!IsValidThemeName(name))
            {
                throw new ArgumentException("Not a valid theme name.", nameof(name));
            }

            _name = name;
        }

        public virtual string GetName()
        {
            return _name;
        }

        public static bool IsValidThemeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (char c in name)
            {
                if (IllegalChars.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }

    }
}