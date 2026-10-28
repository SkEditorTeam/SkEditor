using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Core.Common;

/// <summary>
/// Used for wrapping up a stream for reporting its progress
/// </summary>
public sealed class ProgressStream(Stream innerStream, IProgress<long>? progress) : Stream
{
    private long _totalRead;

    public override bool CanRead => innerStream.CanRead;
    public override bool CanSeek => innerStream.CanSeek;
    public override bool CanWrite => innerStream.CanWrite;
    public override long Length => innerStream.Length;

    public override long Position
    {
        get => innerStream.Position;
        set => innerStream.Position = value;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int bytesRead = await innerStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (bytesRead <= 0)
        {
            return bytesRead;
        }

        _totalRead += bytesRead;
        progress?.Report(_totalRead);
        return bytesRead;
    }

    public override void Flush()
    {
        innerStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return innerStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return innerStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        innerStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        innerStream.Write(buffer, offset, count);
    }
}