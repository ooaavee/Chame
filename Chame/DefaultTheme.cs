namespace Chame
{
    /// <summary>
    /// 'default' theme
    /// </summary>
    public class DefaultTheme : ITheme
    {
        public virtual string GetName()
        {
            return "default";
        }
    }
}