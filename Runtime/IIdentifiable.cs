#nullable enable

using System;

namespace Popcron
{
    /// <summary>
    /// Makes this object findable by its <see cref="ID"/> when calling <see cref="Everything.TryGetAtPath(ReadOnlySpan{char})"/> or <see cref="Everything.GetWithID(ReadOnlySpan{char})"/>
    /// </summary>
    public interface IIdentifiable
    {
        ReadOnlySpan<char> ID { get; }
    }
}