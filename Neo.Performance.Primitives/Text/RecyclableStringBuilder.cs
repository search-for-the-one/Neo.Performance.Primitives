using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Performance.Primitives.Text
{
    public sealed class RecyclableStringBuilder : IDisposable
    {
#if DEBUG
        private const bool ClearRented = true;
#else
        private const bool ClearRented = false;
#endif

        private const int ChunkSize = 256;
        private const int InitialChunkCount = 32;

        private List<char[]> chunks = new List<char[]>(InitialChunkCount);

        public int Length { get; private set; }

        public RecyclableStringBuilder()
        {
        }

        public RecyclableStringBuilder(RecyclableStringBuilder other)
        {
            Append(other);
        }

        private void ReleaseUnmanagedResources()
        {
            if (chunks == null)
                return;

            ReturnChunks();
            chunks = null;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~RecyclableStringBuilder()
        {
            ReleaseUnmanagedResources();
        }

        public RecyclableStringBuilder Clear()
        {
            VerifyNotDisposed();

            ReturnChunks();
            Length = 0;
            chunks.Clear();

            return this;
        }

        public RecyclableStringBuilder Append(char c)
        {
            VerifyNotDisposed();

            var chunkOffset = Length % ChunkSize;
            if (chunkOffset == 0)
                RentChunk();

            var chunk = chunks.Last();
            chunk[chunkOffset] = c;
            Length++;

            return this;
        }

        public RecyclableStringBuilder Append(char c, int repeatCount)
        {
            VerifyNotDisposed();

            if (repeatCount < 0)
                throw new ArgumentOutOfRangeException(nameof(repeatCount));

            while (repeatCount-- > 0)
            {
                var chunkOffset = Length % ChunkSize;
                if (chunkOffset == 0)
                    RentChunk();

                var chunk = chunks.Last();
                chunk[chunkOffset] = c;
                Length++;
            }

            return this;
        }

        public RecyclableStringBuilder Append(string s)
        {
            VerifyNotDisposed();

            var len = s?.Length ?? 0;
            if (len == 0)
                return this;

            AppendInternal(s.AsSpan());
            return this;
        }

        public RecyclableStringBuilder Append(ReadOnlySpan<char> s)
        {
            VerifyNotDisposed();

            var len = s.Length;
            if (len == 0)
                return this;

            AppendInternal(s);
            return this;
        }

        public RecyclableStringBuilder Append(RecyclableStringBuilder other)
        {
            VerifyNotDisposed();

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var len = other.Length;
            if (len == 0)
                return this;

            var remainder = len;
            foreach (var chunk in other.chunks)
            {
                var count = Math.Min(ChunkSize, remainder);
                AppendInternal(chunk, count);
                remainder -= count;
            }

            return this;
        }

        public override string ToString() => Build();

        private string Build()
        {
            VerifyNotDisposed();

            if (Length == 0)
                return string.Empty;

            var result = new string(char.MinValue, Length);
            for (var i = 0; i < chunks.Count; i++)
            {
                var offset = i * ChunkSize;
                var len = Length - offset;
                var chunk = chunks[i];
                var chunkLen = Math.Min(ChunkSize, len);
                Unsafe.StringCopy(chunk, 0, result, offset, chunkLen);
            }

            return result;
        }

        private void AppendInternal(ReadOnlySpan<char> s, int len = -1)
        {
            var sourceIndex = 0;
            var remainder = len < 0 ? s.Length : len;
            while (remainder > 0)
            {
                var chunkOffset = Length % ChunkSize;
                if (chunkOffset == 0)
                    RentChunk();

                var chunk = chunks.Last();
                var count = Math.Min(ChunkSize - chunkOffset, remainder);
                Unsafe.StringCopy(s, sourceIndex, chunk, chunkOffset, count);
                sourceIndex += count;
                Length += count;
                remainder -= count;
            }
        }

        private void VerifyNotDisposed()
        {
            if (chunks == null)
                throw new ObjectDisposedException(nameof(RecyclableStringBuilder));
        }

        private void RentChunk() => chunks.Add(ArrayPool<char>.Shared.Rent(ChunkSize));

        private void ReturnChunks()
        {
            foreach (var chunk in chunks)
                ArrayPool<char>.Shared.Return(chunk, ClearRented);
        }
    }
}