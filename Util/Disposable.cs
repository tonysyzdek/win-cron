using System;

namespace WinCron.Util
{
    public class Disposable : IDisposable
    {
        public static readonly IDisposable Null = NullDisposable.Instance;

        protected readonly Disposables _disposables = new Disposables();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private class NullDisposable : IDisposable
        {
            public static readonly IDisposable Instance = new NullDisposable();

            private NullDisposable()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}