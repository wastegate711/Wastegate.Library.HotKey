using System;
using System.Collections.Generic;
using System.Threading;

namespace Wastegate.Library.HotKey
{
    /// <summary>
    /// Потокобезопасный менеджер привязок сочетаний клавиш к действиям приложения.
    /// Менеджер хранит привязки в памяти и не перехватывает системные события клавиатуры.
    /// </summary>
    public sealed class HotKeyManager : IDisposable
    {
        // Блокировка защищает словарь привязок и состояние менеджера от одновременного доступа потоков.
        private readonly object _gate = new object();

        // Каждое сочетание может быть зарегистрировано только один раз в пределах этого экземпляра менеджера.
        private readonly Dictionary<HotKeyGesture, HotKeyRegistration> _registrations =
            new Dictionary<HotKeyGesture, HotKeyRegistration>();

        private bool _disposed;

        /// <summary>
        /// Регистрирует обработчик для указанного сочетания клавиш.
        /// Обработчик сохраняется до снятия привязки или вызова <see cref="Dispose"/> у менеджера.
        /// </summary>
        /// <param name="gesture">Сочетание клавиш, которое должно вызывать действие.</param>
        /// <param name="handler">Действие, вызываемое после распознавания сочетания.</param>
        /// <returns>Объект регистрации, который можно использовать для снятия этой привязки.</returns>
        /// <exception cref="ArgumentNullException">Параметр handler равен null.</exception>
        /// <exception cref="InvalidOperationException">Такое сочетание уже зарегистрировано.</exception>
        /// <exception cref="ObjectDisposedException">Менеджер уже освобожден методом Dispose.</exception>
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

        /// <summary>
        /// Передает распознанное приложением сочетание менеджеру и вызывает связанный обработчик.
        /// Блокировка снимается до вызова обработчика, чтобы обработчик не мог вызвать взаимную блокировку.
        /// Исключение обработчика передается вызывающему коду без перехвата.
        /// </summary>
        /// <param name="gesture">Сочетание клавиш, полученное из обработчика ввода приложения.</param>
        /// <returns><see langword="true" />, если привязка найдена и обработчик вызван; иначе <see langword="false" />.</returns>
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

        /// <summary>
        /// Снимает все зарегистрированные привязки и переводит менеджер в освобожденное состояние.
        /// Повторный вызов не вызывает исключений и не меняет состояние других объектов.
        /// </summary>
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

        /// <summary>
        /// Снимает только указанную привязку; метод вызывается объектом HotKeyRegistration.
        /// </summary>
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

        // Единая проверка состояния используется перед добавлением привязки.
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HotKeyManager));
            }
        }
    }

    /// <summary>
    /// Представляет конкретную привязку сочетания клавиш к обработчику.
    /// Вызов Dispose снимает только эту привязку, не затрагивая остальные сочетания менеджера.
    /// </summary>
    public sealed class HotKeyRegistration : IDisposable
    {
        private HotKeyManager _owner;

        internal HotKeyRegistration(HotKeyManager owner, HotKeyGesture gesture, Action handler)
        {
            _owner = owner;
            Gesture = gesture;
            Handler = handler;
        }

        /// <summary>Получает сочетание клавиш, связанное с этой регистрацией.</summary>
        public HotKeyGesture Gesture { get; }

        // Обработчик закрыт от внешнего вызова и используется только методом TryHandle менеджера.
        internal Action Handler { get; }

        /// <summary>
        /// Снимает привязку из родительского менеджера. Повторный вызов безопасен.
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.Unregister(this);
        }
    }
}
