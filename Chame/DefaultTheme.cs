namespace Chame
{
    /// <summary>
    /// 'default' theme
    /// </summary>
    public sealed class DefaultTheme : ITheme
    {
        public const string Name = "default";

        public string GetName()
        {
            return Name;
        }
    }
}