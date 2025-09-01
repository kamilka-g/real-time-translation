using System.Net.Http.Headers;
using System.Text.Json;
using EchoTravel.Api.Ports;

namespace EchoTravel.Api.Adapters;

/// <summary>
/// HTTP client that calls a FastAPI speech‑to‑text service.  Expects the service to expose a
/// POST /transcribe endpoint which accepts a multipart/form‑data upload named "file" and returns JSON
/// with a "text" and optional "lang" property.
/// </summary>
public sealed class FastApiSttClient : ISttClient
{
    private readonly HttpClient _http;

    public FastApiSttClient(HttpClient http) => _http = http;

    public async Task<(string Text, string Lang)> TranscribeAsync(Stream audio, string fileName, string contentType, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(audio);
        content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "audio/wav" : contentType);
        form.Add(content, "file", fileName);

        using var response = await _http.PostAsync("/transcribe", form, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var text = doc.RootElement.GetProperty("text").GetString() ?? string.Empty;
        var lang = doc.RootElement.TryGetProperty("lang", out var l)
            ? l.GetString() ?? "auto"
            : "auto";
        return (text.Trim(), lang);
    }
}
