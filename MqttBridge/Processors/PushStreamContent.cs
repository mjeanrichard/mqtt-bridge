using System.Net;

namespace MqttBridge.Processors;

public class PushStreamContent : HttpContent
{
    private readonly Func<Stream, Task> _callback;

    public PushStreamContent(Func<Stream, Task> callback)
    {
        _callback = callback;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        return _callback(stream);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}