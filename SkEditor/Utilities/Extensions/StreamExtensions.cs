using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Extensions;

public static class StreamExtensions
{
    public static async ValueTask<bool> IsLikelyBinaryAsync(this Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new InvalidOperationException("Stream must support seeking to check if it's binary.");
        }

        const int bufferSize = 8192;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bufferSize));

            bool isBinary = buffer.AsSpan(0, bytesRead).Contains((byte)0);

            stream.Position = 0;

            return isBinary;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}