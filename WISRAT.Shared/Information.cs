using System.Reflection;

namespace WISRAT.Shared
{
    public static class Information
    {
        public static readonly string SharedVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
    }
}