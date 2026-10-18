using System;
using System.IO;

namespace SkEditor.Core.Extensions;

public static class StreamExtensions
{
    public static bool IsLikelyBinary(this Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new InvalidOperationException("Stream must support seeking to check if it's binary.");
        }

        Span<byte> buffer = stackalloc byte[1024];

        long originalPosition = stream.Position;
        try
        {
            int bytesRead = stream.Read(buffer);
            return buffer[..bytesRead].Contains((byte)0);
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }
}