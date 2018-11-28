namespace WinCron.Util
{
    public interface IConfiguration
    {
        string ResolveString(string key);
    }
}