namespace Chame
{
    public interface ITheme
    {
        /// <summary>
        /// Gets a theme name.
        /// </summary>
        /// <remarks>
        /// A theme name must be a valid file system directory name, so it cannot
        /// - contain any of the following characters: /?:%\*"<>|#%
        /// - contain Unicode control characters
        /// - contain invalid surrogate characters 
        /// - be system reserved names, including 'CON', 'AUX', 'PRN', 'COM1' or 'LPT1'
        /// - be '.' or '..'
        /// </remarks>
        /// <returns>theme name</returns>
        string GetName();
    }

    /// <summary>
    /// 'default' theme
    /// </summary>
    internal sealed class DefaultTheme : ITheme
    {
        public string GetName()
        {
            return "default";
        }
    }
}
