using System.Buffers;
using System.IO.Pipelines;

namespace TidyHPC.ASP.LiteKestrelServers.HttpSessions;

public class KestrelStream(PipeWriter pipeWriter) : Stream
{
    public PipeWriter PipeWriter { get; } = pipeWriter;

    public override bool CanRead => throw new NotImplementedException();

    public override bool CanSeek => throw new NotImplementedException();

    public override bool CanWrite => true;

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Flush()
    {

    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        PipeWriter.Write(buffer.AsSpan(offset, count));
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await PipeWriter.WriteAsync(buffer, cancellationToken);
    }
}
