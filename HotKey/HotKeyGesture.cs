using System;

namespace Wastegate.Library.HotKey
{
    /// <summary>
    /// Задает модификаторы, которые могут входить в сочетание клавиш.
    /// Значения можно объединять побитовым ИЛИ для задания нескольких модификаторов.
    /// </summary>
    [Flags]
    public enum HotKeyModifiers
    {
        /// <summary>Сочетание не содержит модификаторов.</summary>
        None = 0,

        /// <summary>Клавиша Alt должна быть нажата вместе с основной клавишей.</summary>
        Alt = 1,

        /// <summary>Клавиша Control должна быть нажата вместе с основной клавишей.</summary>
        Control = 2,

        /// <summary>Клавиша Shift должна быть нажата вместе с основной клавишей.</summary>
        Shift = 4,

        /// <summary>Системная клавиша Windows должна быть нажата вместе с основной клавишей.</summary>
        Windows = 8
    }

    /// <summary>
    /// Описывает неизменяемое сочетание основной клавиши и модификаторов.
    /// Экземпляр можно использовать как ключ словаря, поскольку реализованы <see cref="IEquatable{T}"/>,
    /// согласованные методы Equals/GetHashCode и операторы равенства.
    /// </summary>
    public readonly struct HotKeyGesture : IEquatable<HotKeyGesture>
    {
        private const HotKeyModifiers ValidModifiers =
            HotKeyModifiers.Alt | HotKeyModifiers.Control | HotKeyModifiers.Shift | HotKeyModifiers.Windows;

        /// <summary>
        /// Создает описание сочетания клавиш после проверки переданных значений.
        /// </summary>
        /// <param name="key">Основная клавиша сочетания. Должна быть определенным значением перечисления ConsoleKey.</param>
        /// <param name="modifiers">Набор модификаторов. По умолчанию модификаторы отсутствуют.</param>
        /// <exception cref="ArgumentOutOfRangeException">Клавиша недопустима или содержит неописанный модификатор.</exception>
        public HotKeyGesture(ConsoleKey key, HotKeyModifiers modifiers = HotKeyModifiers.None)
        {
            if (!Enum.IsDefined(typeof(ConsoleKey), key))
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            if ((modifiers & ~ValidModifiers) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(modifiers));
            }

            Key = key;
            Modifiers = modifiers;
        }

        /// <summary>Получает основную клавишу сочетания.</summary>
        public ConsoleKey Key { get; }

        /// <summary>Получает набор модификаторов сочетания.</summary>
        public HotKeyModifiers Modifiers { get; }

        /// <summary>
        /// Сравнивает текущее сочетание с другим сочетанием по основной клавише и модификаторам.
        /// </summary>
        /// <param name="other">Сочетание, с которым выполняется сравнение.</param>
        /// <returns><see langword="true" />, если клавиша и модификаторы совпадают; иначе <see langword="false" />.</returns>
        public bool Equals(HotKeyGesture other)
        {
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        /// <summary>Определяет, представляет ли переданный объект такое же сочетание клавиш.</summary>
        /// <param name="obj">Объект для сравнения.</param>
        /// <returns><see langword="true" />, если объект является HotKeyGesture с теми же значениями.</returns>
        public override bool Equals(object obj)
        {
            return obj is HotKeyGesture other && Equals(other);
        }

        /// <summary>
        /// Вычисляет хеш сочетания. Комбинация хеша клавиши и модификаторов согласована с методом Equals.
        /// </summary>
        /// <returns>Хеш-код сочетания.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Key * 397) ^ (int)Modifiers;
            }
        }

        /// <summary>Сравнивает два сочетания на равенство.</summary>
        public static bool operator ==(HotKeyGesture left, HotKeyGesture right)
        {
            return left.Equals(right);
        }

        /// <summary>Сравнивает два сочетания на неравенство.</summary>
        public static bool operator !=(HotKeyGesture left, HotKeyGesture right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Возвращает читаемое представление сочетания. Например, Control+Shift+S.
        /// </summary>
        /// <returns>Текстовое представление клавиши и модификаторов.</returns>
        public override string ToString()
        {
            return Modifiers == HotKeyModifiers.None ? Key.ToString() : $"{Modifiers}+{Key}";
        }
    }
}
