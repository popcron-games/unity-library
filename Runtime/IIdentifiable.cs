using System;

namespace Popcron
{
    /// <summary>
    /// Makes this object testable against another <see cref="ReadOnlySpan{char}"/>
    /// </summary>
    public interface IIdentifiable
    {
        ReadOnlySpan<char> ID { get; }
    }
}