#nullable enable
using System;

namespace Popcron
{
    public static class SpanExtensions
    {
        public static int GetSpanHashCode(this ReadOnlySpan<char> span)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < span.Length; i++)
                {
                    hash = hash * 31 + span[i];
                }

                return hash;
            }
        }

        public static int GetSpanHashCode(this string text) => GetSpanHashCode(text.AsSpan());
    }
}