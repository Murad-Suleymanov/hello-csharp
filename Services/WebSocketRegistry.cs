using System.Collections.Concurrent;
using System.Threading.Channels;

namespace HelloCSharp;

public class WebSocketSession
{
    public Channel<string> Outbox { get; } = Channel.CreateUnbounded<string>();
    public CancellationTokenSource Cts { get; } = new();
}

public class WebSocketRegistry
{
    private readonly ConcurrentDictionary<string, WebSocketSession> _sessions = new();

    public string Register(out WebSocketSession session)
    {
        var id = Guid.NewGuid().ToString("N");
        session = new WebSocketSession();
        _sessions[id] = session;
        return id;
    }

    public bool Send(string id, string message)
    {
        if (!_sessions.TryGetValue(id, out var session)) return false;
        session.Outbox.Writer.TryWrite(message);
        return true;
    }

    public bool Cancel(string id)
    {
        if (!_sessions.TryRemove(id, out var session)) return false;
        session.Outbox.Writer.Complete();
        session.Cts.Cancel();
        session.Cts.Dispose();
        return true;
    }

    public void Remove(string id)
    {
        if (_sessions.TryRemove(id, out var session))
        {
            session.Outbox.Writer.TryComplete();
            session.Cts.Dispose();
        }
    }

    public IEnumerable<string> ActiveIds => _sessions.Keys;
}
