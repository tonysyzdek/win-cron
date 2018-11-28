namespace WinCron.Logging
{
    public interface IErrorReporter
    {
        void Write(string text);
    }
}