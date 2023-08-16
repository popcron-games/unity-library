using System;

namespace Popcron
{
    /// <summary>
    /// Makes this object testable against a <see cref="ReadOnlySpan{char}"/>
    /// </summary>
    public interface ICanBeIdentified
    {
        bool Equals(ReadOnlySpan<char> value);
    }
}