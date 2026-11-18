using System;

namespace Wastegate.Lybrary.HotKey
{
    [Flags]
    public enum HotKeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }

    public readonly struct HotKeyGesture : IEquatable<HotKeyGesture>
    {
        private const HotKeyModifiers ValidModifiers =
            HotKeyModifiers.Alt | HotKeyModifiers.Control | HotKeyModifiers.Shift | HotKeyModifiers.Windows;

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

        public ConsoleKey Key { get; }

        public HotKeyModifiers Modifiers { get; }

        public bool Equals(HotKeyGesture other)
        {
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        public override bool Equals(object obj)
        {
            return obj is HotKeyGesture other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Key * 397) ^ (int)Modifiers;
            }
        }

        public static bool operator ==(HotKeyGesture left, HotKeyGesture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HotKeyGesture left, HotKeyGesture right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return Modifiers == HotKeyModifiers.None ? Key.ToString() : $"{Modifiers}+{Key}";
        }
    }
}
