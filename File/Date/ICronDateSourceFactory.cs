namespace WinCron.File.Date
{
    public interface ICronDateSourceFactory
    {
        bool TryGetValue(string token, out ICronDateSource source);
    }
}