using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace WinCron.Util
{
    public class Disposables : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly HashSet<IDisposable> _hash = new HashSet<IDisposable>();
        private readonly List<IDisposable> _items = new List<IDisposable>();
        
        public void Dispose()
        {
            while (_items.Count > 0)
            {
                try
                {
                    var index = _items.Count-1;
                    var disposable = _items[index];
                    _items.RemoveAt(index);
                    disposable.Dispose();
                }
                catch (Exception e)
                {
                    Log.Warn("Exception occurred while disposing: ", e);
                }
            }

            _items.Clear();
            _hash.Clear();
        }

        public void Add(Action item)
        {
            Add(new DisposableAction(item));
        }

        public T Add<T>(T item) where T : IDisposable
        {
            if (_hash.Add(item))
            {
                _items.Add(item);
            }

            return item;
        }
    }
}
