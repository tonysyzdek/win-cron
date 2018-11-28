using System;

namespace WinCron.Util
{
    public class DisposableAction : IDisposable
    {
        private readonly Action _disposer;

        public DisposableAction(Action disposer)
        {
            _disposer = disposer;
        }

        public void Dispose()
        {
            _disposer();
        }

        protected bool Equals(DisposableAction other)
        {
            return Equals(_disposer, other._disposer);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DisposableAction) obj);
        }

        public override int GetHashCode()
        {
            return (_disposer != null ? _disposer.GetHashCode() : 0);
        }

        public static bool operator ==(DisposableAction left, DisposableAction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DisposableAction left, DisposableAction right)
        {
            return !Equals(left, right);
        }
    }
}