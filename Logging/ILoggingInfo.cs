namespace WinCron.Logging
{
    public interface ILoggingInfo
    {
        string FilePath { get; }
        string BaseDirectory { get; }
    }
}