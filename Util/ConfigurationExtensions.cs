namespace WinCron.Util
{
    public static class ConfigurationExtensions
    {
        public static int ResolveInt(this IConfiguration config, string key)
        {
            return int.Parse(config.ResolveString(key));
        }
    }
}