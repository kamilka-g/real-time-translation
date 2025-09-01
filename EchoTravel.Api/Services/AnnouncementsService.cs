using EchoTravel.Api.Hubs;
using EchoTravel.Api.Ports;
using Microsoft.AspNetCore.SignalR;

namespace EchoTravel.Api.Services;

/// <summary>
/// Orchestrates the end‑to‑end flow of processing an announcement: transcribing audio, translating the
/// resulting text and broadcasting it to the appropriate SignalR group.
/// </summary>
public sealed class AnnouncementsService
{
    private readonly ISttClient _sttClient;
    private readonly Translator _translator;
    private readonly IHubContext<AnnouncementsHub> _hub;

    public AnnouncementsService(ISttClient sttClient, Translator translator, IHubContext<AnnouncementsHub> hub) =>
        (_sttClient, _translator, _hub) = (sttClient, translator, hub);

    /// <summary>
    /// Processes an uploaded audio file: recognises the speech, translates it and publishes the
    /// announcement to all subscribed clients.
    /// </summary>
    /// <param name="trainId">Train identifier.</param>
    /// <param name="audio">Audio stream.</param>
    /// <param name="fileName">Original file name.</param>
    /// <param name="contentType">MIME type.</param>
    /// <param name="toLang">Target language code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tuple of original recognised text and translated text.</returns>
    public async Task<(string Original, string Translated)> ProcessAsync(string trainId, Stream audio, string fileName, string contentType, string toLang, CancellationToken ct)
    {
        var (text, _) = await _sttClient.TranscribeAsync(audio, fileName, contentType, ct);
        var translated = await _translator.TranslateAsync(text, toLang, ct);
        var groupName = TrainGroups.GetTrainGroupName(trainId, null);
        await _hub.Clients.Group(groupName).SendAsync("announcement", new
        {
            trainId,
            original = text,
            translated,
            to = toLang,
            timestamp = DateTimeOffset.UtcNow
        }, ct);
        return (text, translated);
    }
}
