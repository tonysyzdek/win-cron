using System;

namespace WinCron.Logging
{
    public class ConsoleErrorReporter : IErrorReporter
    {
        public void Write(string text)
        {
            Console.Out.WriteLine(text);
            Console.Out.Flush();
        }
    }
}