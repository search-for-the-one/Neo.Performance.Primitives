using System;

namespace Neo.Performance.Primitives.Text
{
    public static unsafe class Unsafe
    {
        public static void StringCopy(ReadOnlySpan<char> source, int sourceOffset, Span<char> destination, int destinationOffset, int length)
        {
            fixed (char* destPtr = destination)
            fixed (char* sourcePtr = source)
            {
                StringCopy(sourcePtr, sourceOffset, destPtr, destinationOffset, destination.Length, length);
            }
        }

        public static void StringCopy(ReadOnlySpan<char> source, int sourceOffset, char[] destination, int destinationOffset, int length)
        {
            fixed (char* destPtr = destination)
            fixed (char* sourcePtr = source)
            {
                StringCopy(sourcePtr, sourceOffset, destPtr, destinationOffset, destination.Length, length);
            }
        }

        public static void StringCopy(ReadOnlySpan<char> source, int sourceOffset, string destination, int destinationOffset, int length)
        {
            fixed (char* destPtr = destination)
            fixed (char* sourcePtr = source)
            {
                StringCopy(sourcePtr, sourceOffset, destPtr, destinationOffset, destination.Length, length);
            }
        }

        public static void StringCopy(char[] source, int sourceOffset, char[] destination, int destinationOffset, int length)
        {
            fixed (char* destPtr = destination)
            fixed (char* sourcePtr = source)
            {
                StringCopy(sourcePtr, sourceOffset, destPtr, destinationOffset, destination.Length, length);
            }
        }

        public static void StringCopy(char[] source, int sourceOffset, string destination, int destinationOffset, int length)
        {
            fixed (char* destPtr = destination)
            fixed (char* sourcePtr = source)
            {
                StringCopy(sourcePtr, sourceOffset, destPtr, destinationOffset, destination.Length, length);
            }
        }

        public static void StringCopy(string source, int sourceOffset, char[] destination, int destinationOffset, int length)
        {
            fixed (char* destPtr = destination)
            fixed (char* sourcePtr = source)
            {
                StringCopy(sourcePtr, sourceOffset, destPtr, destinationOffset, destination.Length, length);
            }
        }

        public static void StringCopy(string source, int sourceOffset, string destination, int destinationOffset, int length)
        {
            fixed (char* destPtr = destination)
            fixed (char* sourcePtr = source)
            {
                StringCopy(sourcePtr, sourceOffset, destPtr, destinationOffset, destination.Length, length);
            }
        }

        public static void StringCopy(char* sourcePtr, int sourceOffset, char* destPtr, int destinationOffset, int destinationLength, int length)
        {
            Buffer.MemoryCopy(sourcePtr + sourceOffset,
                destPtr + destinationOffset, (destinationLength - destinationOffset) * sizeof(char),
                length * sizeof(char));
        }
    }
}
