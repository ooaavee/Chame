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
        private static readonly char[] IllegalCharsInThemeName = Path.GetInvalidFileNameChars();

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

        /// <summary>
        /// Checks if the specified value would be a valid theme name.
        /// </summary>
        /// <param name="name">a value to check</param>
        /// <returns>true/false</returns>
        public static bool IsValidThemeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (char c in name)
            {
                if (IllegalCharsInThemeName.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }

    }
}