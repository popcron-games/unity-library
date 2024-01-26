#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class Djb2HashCodeExtension
{
    public unsafe static int GetDjb2HashCode(this string str)
    {
        int length = str.Length;
        fixed (char* source = str)
        {
            return GetDjb2HashCode<char>(source, length);
        }
    }

    public unsafe static int GetDjb2HashCode<T>(this ReadOnlySpan<T> span) where T : unmanaged
    {
        int length = span.Length;
        void* source = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        return GetDjb2HashCode<T>(source, length);
    }

    public unsafe static int GetDjb2HashCode<T>(this Span<T> span) where T : unmanaged
    {
        int length = span.Length;
        void* source = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        return GetDjb2HashCode<T>(source, length);
    }

    private unsafe static int GetDjb2HashCode<T>(void* source, int length) where T : unmanaged
    {
        int hash = 17;
        int offset = 0;

        while (length >= 8)
        {
            // Doing a left shift by 5 and adding is equivalent to multiplying by 33.
            // This is preferred for performance reasons, as when working with integer
            // values most CPUs have higher latency for multiplication operations
            // compared to a simple shift and add. For more info on this, see the
            // details for imul, shl, add: https://gmplib.org/~tege/x86-timing.pdf.
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 0).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 1).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 2).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 3).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 4).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 5).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 6).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 7).GetHashCode());

            length -= 8;
            offset += 8;
        }

        if (length >= 4)
        {
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 0).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 1).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 2).GetHashCode());
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset + 3).GetHashCode());

            length -= 4;
            offset += 4;
        }

        while (length > 0)
        {
            hash = unchecked(((hash << 5) + hash) ^ ReadArrayElement<T>(source, offset).GetHashCode());

            length -= 1;
            offset += 1;
        }

        return hash;
    }

    private unsafe static T ReadArrayElement<T>(void* source, int index)
    {
        return Unsafe.Read<T>((byte*)source + (long)index * (long)Unsafe.SizeOf<T>());
    }
}