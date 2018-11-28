namespace WinCron.File.Date
{
    public class EmptyDateSourceFactory : ICronDateSourceFactory
    {
        public bool TryGetValue(string token, out ICronDateSource source)
        {
            source = null;
            return false;
        }
    }
}