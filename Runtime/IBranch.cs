#nullable enable
using System;

namespace Popcron.Lib
{
    /// <summary>
    /// Allows for branching out when using <see cref="Everything.TryGetAtPath(ReadOnlySpan{char}, out object?)"/>.
    /// </summary>
    public interface IBranch
    {
        bool TryGetChild(ReadOnlySpan<char> id, out object? value);
    }
}