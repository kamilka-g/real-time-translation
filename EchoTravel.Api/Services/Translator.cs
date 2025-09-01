using System.Net.Http.Json;
using System.Text.Json;

namespace EchoTravel.Api.Services;

/// <summary>
/// Encapsulates calls to the Azure Cognitive Services translation API.  This class abstracts
/// away HTTP details so that the rest of the application simply requests a translation.
/// </summary>
public sealed class Translator
{
    private readonly HttpClient _http;
    private readonly string _subscriptionKey;
    private readonly string _region;

    public Translator(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _subscriptionKey = configuration["Translator:Key"] ?? string.Empty;
        _region = configuration["Translator:Region"] ?? "westeurope";
    }

    /// <summary>
    /// Translates the given text into the specified language.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="to">The ISO code of the target language.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<string> TranslateAsync(string text, string to, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        var url = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={to}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
        request.Headers.Add("Ocp-Apim-Subscription-Region", _region);
        request.Content = JsonContent.Create(new[] { new { Text = text } });
        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement[0].GetProperty("translations")[0].GetProperty("text").GetString() ?? text;
    }
}
