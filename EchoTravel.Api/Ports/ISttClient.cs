namespace EchoTravel.Api.Ports;

/// <summary>
/// Abstraction for services that can transcribe audio into text.  Implementations may call external
/// APIs or microservices.  This decouples the rest of the application from any particular STT provider.
/// </summary>
public interface ISttClient
{
    /// <summary>
    /// Transcribes an audio stream to text.
    /// </summary>
    /// <param name="audio">The audio stream to transcribe.</param>
    /// <param name="fileName">An optional file name for the uploaded audio (used for diagnostics).</param>
    /// <param name="contentType">The MIME type of the audio (e.g. audio/wav).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recognised text and detected language code.</returns>
    Task<(string Text, string Lang)> TranscribeAsync(Stream audio, string fileName, string contentType, CancellationToken ct = default);
}
