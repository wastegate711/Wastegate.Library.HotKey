using System;
using System.Collections.Generic;
using System.Threading;

namespace Wastegate.Lybrary.HotKey
{
    public sealed class HotKeyManager : IDisposable
    {
        private readonly object _gate = new object();
        private readonly Dictionary<HotKeyGesture, HotKeyRegistration> _registrations =
            new Dictionary<HotKeyGesture, HotKeyRegistration>();
        private bool _disposed;

        public HotKeyRegistration Register(HotKeyGesture gesture, Action handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            lock (_gate)
            {
                ThrowIfDisposed();

                if (_registrations.ContainsKey(gesture))
                {
                    throw new InvalidOperationException($"The hot key '{gesture}' is already registered.");
                }

                var registration = new HotKeyRegistration(this, gesture, handler);
                _registrations.Add(gesture, registration);
                return registration;
            }
        }

        public bool TryHandle(HotKeyGesture gesture)
        {
            Action handler;

            lock (_gate)
            {
                if (_disposed || !_registrations.TryGetValue(gesture, out var registration))
                {
                    return false;
                }

                handler = registration.Handler;
            }

            handler();
            return true;
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _registrations.Clear();
            }
        }

        internal void Unregister(HotKeyRegistration registration)
        {
            lock (_gate)
            {
                if (_registrations.TryGetValue(registration.Gesture, out var current) &&
                    ReferenceEquals(current, registration))
                {
                    _registrations.Remove(registration.Gesture);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HotKeyManager));
            }
        }
    }

    public sealed class HotKeyRegistration : IDisposable
    {
        private HotKeyManager _owner;

        internal HotKeyRegistration(HotKeyManager owner, HotKeyGesture gesture, Action handler)
        {
            _owner = owner;
            Gesture = gesture;
            Handler = handler;
        }

        public HotKeyGesture Gesture { get; }

        internal Action Handler { get; }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.Unregister(this);
        }
    }
}
