namespace Chame
{
    /// <summary>
    /// Caching modes
    /// </summary>
    public enum CachingModes
    {
        /// <summary>
        /// No, caching is disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// Yes, caching is enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// Caching is enabled, but disabled when working on development environment.
        /// </summary>
        EnabledButDisabledOnDev
    }
}