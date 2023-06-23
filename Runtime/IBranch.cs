#nullable enable
using System;

namespace Popcron
{
    /// <summary>
    /// Allows for branching out when using <see cref="Everything.TryGetAtPath(ReadOnlySpan{char})"/>.
    /// </summary>
    public interface IBranch
    {
        bool TryGetChild(ReadOnlySpan<char> id, out object? value);
    }
}