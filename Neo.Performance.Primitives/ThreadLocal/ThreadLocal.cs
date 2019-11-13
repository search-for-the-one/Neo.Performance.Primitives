using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Neo.Performance.Primitives.ThreadLocal
{
    /// <summary>
    /// ThreadLocalSharedSpan behaves like a shadow stack without the limit of stack size (4MB for 64-bit).
    /// Usage is expected to be like how stackalloc would be used except Get() cannot be called multiple times per scope.
    /// Usage pattern to follow: using (ThreadLocalSharedSpan&lt;Foo&gt;.Get&lt;int&gt;(5, out var array)) { }
    /// </summary>
    /// <typeparam name="TClass">The class you want this shadow stack for (i.e. the shadow stack is keyed against a class)</typeparam>
    // ReSharper disable once UnusedTypeParameter
    public static class ThreadLocalSharedSpan<TClass>
    {
        private const int InitialSize = 64 * 1024; // 64KB

        // ReSharper disable once StaticMemberInGenericType
        [ThreadStatic] private static Backing backing;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Scope Get<T>(int count, out Span<T> result) where T : struct
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
            {
                result = Span<T>.Empty;
                return new Scope(true);
            }

            var length = count * Unsafe.SizeOf<T>();
            Allocate(length);

            result = MemoryMarshal.Cast<byte, T>(new Span<byte>(backing.Array).Slice(0, length));
            return new Scope(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Scope Get<T1, T2>(int count1, out Span<T1> result1, int count2, out Span<T2> result2) where T1 : struct where T2 : struct
        {
            if (count1 < 0)
                throw new ArgumentOutOfRangeException(nameof(count1));

            if (count2 < 0)
                throw new ArgumentOutOfRangeException(nameof(count2));

            var length1 = count1 * Unsafe.SizeOf<T1>();
            var length2 = count2 * Unsafe.SizeOf<T2>();

            var length = length1 + length2;
            if (length == 0)
            {
                result1 = Span<T1>.Empty;
                result2 = Span<T2>.Empty;
                return new Scope(true);
            }

            Allocate(length);

            result1 = length1 > 0 ? MemoryMarshal.Cast<byte, T1>(new Span<byte>(backing.Array).Slice(0, length1)) : Span<T1>.Empty;
            result2 = length2 > 0 ? MemoryMarshal.Cast<byte, T2>(new Span<byte>(backing.Array).Slice(length1, length2)) : Span<T2>.Empty;
            return new Scope(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Scope Get<T1, T2, T3>(int count1, out Span<T1> result1, int count2, out Span<T2> result2, int count3, out Span<T3> result3) where T1 : struct where T2 : struct where T3 : struct
        {
            if (count1 < 0)
                throw new ArgumentOutOfRangeException(nameof(count1));

            if (count2 < 0)
                throw new ArgumentOutOfRangeException(nameof(count2));

            if (count3 < 0)
                throw new ArgumentOutOfRangeException(nameof(count3));

            var length1 = count1 * Unsafe.SizeOf<T1>();
            var length2 = count2 * Unsafe.SizeOf<T2>();
            var length3 = count3 * Unsafe.SizeOf<T3>();

            var length = length1 + length2 + length3;
            if (length == 0)
            {
                result1 = Span<T1>.Empty;
                result2 = Span<T2>.Empty;
                result3 = Span<T3>.Empty;
                return new Scope(true);
            }

            Allocate(length);

            result1 = length1 > 0 ? MemoryMarshal.Cast<byte, T1>(new Span<byte>(backing.Array).Slice(0, length1)) : Span<T1>.Empty;
            result2 = length2 > 0 ? MemoryMarshal.Cast<byte, T2>(new Span<byte>(backing.Array).Slice(length1, length2)) : Span<T2>.Empty;
            result3 = length3 > 0 ? MemoryMarshal.Cast<byte, T3>(new Span<byte>(backing.Array).Slice(length1 + length2, length3)) : Span<T3>.Empty;
            return new Scope(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Allocate(int length)
        {
            if (backing.Array == null || backing.Array.Length < length)
                backing.Array = new byte[GetNextMultiple(length, InitialSize)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNextMultiple(int value, int multiple)
        {
            // Note: This only works for value > 0 (i.e. it is optimised for our use case in this class)
            return (value + multiple - 1) / multiple * multiple;
        }

        public struct Scope : IDisposable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Scope(bool inUse)
            {
                if (backing.InUse)
                    throw new InvalidOperationException("Shared span is already in use");

                backing.InUse = inUse;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => backing.InUse = false;
        }

        private struct Backing
        {
            public bool InUse;
            public byte[] Array;
        }
    }
}
