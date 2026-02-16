using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AquaSync.Eheim.Exceptions;

namespace AquaSync.Eheim.Transport;

/// <summary>
///     WebSocket transport that connects to an EHEIM hub and streams incoming JSON messages.
/// </summary>
internal sealed class EheimWebSocketTransport : IEheimTransport
{
    private readonly Subject<JsonNode> _messages = new();
    private CancellationTokenSource? _receiveCts;
    private Task? _receiveLoop;
    private ClientWebSocket? _ws;

    public bool IsConnected => _ws?.State == WebSocketState.Open;

    public IObservable<JsonNode> Messages => _messages.AsObservable();

    public async Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken = default)
    {
        _ws = new ClientWebSocket();
        try
        {
            await _ws.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException)
        {
            throw new EheimConnectionException($"Failed to connect to {endpoint}", ex);
        }

        _receiveCts = new CancellationTokenSource();
        _receiveLoop = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token), CancellationToken.None);
    }

    public async Task DisconnectAsync()
    {
        if (_receiveCts is not null) await _receiveCts.CancelAsync().ConfigureAwait(false);

        if (_ws is { State: WebSocketState.Open })
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);
            }
            catch (WebSocketException)
            {
                // Best-effort close
            }

        if (_receiveLoop is not null)
            try
            {
                await _receiveLoop.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
    }

    public async Task SendAsync(JsonObject message, CancellationToken cancellationToken = default)
    {
        if (_ws is not { State: WebSocketState.Open })
            throw new EheimCommunicationException("WebSocket is not connected.");

        var json = message.ToJsonString();
        var bytes = Encoding.UTF8.GetBytes(json);
        try
        {
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException)
        {
            throw new EheimCommunicationException("Failed to send message.", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        _messages.Dispose();
        _receiveCts?.Dispose();
        _ws?.Dispose();
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        try
        {
            while (!cancellationToken.IsCancellationRequested && _ws is { State: WebSocketState.Open })
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await _ws.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                        return;
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType != WebSocketMessageType.Text)
                    continue;

                var text = Encoding.UTF8.GetString(ms.ToArray());
                EmitParsedMessages(text);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
    }

    private void EmitParsedMessages(string text)
    {
        try
        {
            var node = JsonNode.Parse(text);
            if (node is JsonArray array)
            {
                foreach (var item in array)
                    if (item is not null)
                        _messages.OnNext(item);
            }
            else if (node is not null)
            {
                _messages.OnNext(node);
            }
        }
        catch (JsonException)
        {
            // Malformed JSON — skip
        }
    }
}
