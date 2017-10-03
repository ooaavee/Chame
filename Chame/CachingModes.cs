namespace Chame
{
    /// <summary>
    /// Caching modes
    /// </summary>
    public enum CachingModes
    {
        /// <summary>
        /// Caching is disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// Caching is enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// Caching is enabled, but disabled when working on development environment.
        /// </summary>
        DisabledOnDevelopmentOtherwiseEnabled
    }
}