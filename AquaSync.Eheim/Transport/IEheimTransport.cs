using System.Text.Json.Nodes;

namespace AquaSync.Eheim.Transport;

/// <summary>
///     Abstraction over the WebSocket connection to an EHEIM hub.
/// </summary>
internal interface IEheimTransport : IAsyncDisposable
{
    bool IsConnected { get; }
    IObservable<JsonNode> Messages { get; }
    Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task SendAsync(JsonObject message, CancellationToken cancellationToken = default);
}
