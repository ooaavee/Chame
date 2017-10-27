namespace Chame
{
    public interface IThemeInfo
    {
        /// <summary>
        /// Gets a theme identifier.
        /// </summary>
        /// <remarks>
        /// A theme identifier must be a valid file system directory name, so it cannot
        /// - contain any of the following characters: /?:%\*"<>|#%
        /// - contain Unicode control characters
        /// - contain invalid surrogate characters 
        /// - be system reserved names, including 'CON', 'AUX', 'PRN', 'COM1' or 'LPT1'
        /// - be '.' or '..'
        /// </remarks>
        string Id { get; }
    }
}
