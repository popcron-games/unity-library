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
                int hash = 0;
                for (int i = 0; i < span.Length; i++)
                {
                    hash = hash * 2147483423 + span[i];
                }

                return hash;
            }
        }

        public static int GetSpanHashCode(this string text) => GetSpanHashCode(text.AsSpan());
    }
}