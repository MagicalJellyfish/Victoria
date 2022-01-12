using System.Text.Json;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;

namespace Victoria.Addons;

public sealed record ArtworkResolver(HttpClient _httpClient, ILogger<ArtworkResolver> _logger) {
    private readonly HttpClient _httpClient = _httpClient;
    private readonly ILogger<ArtworkResolver> _logger = _logger;

    public async ValueTask<string> FetchAsync(ILavaTrack track) {
        ArgumentNullException.ThrowIfNull(track, nameof(track));
        var (shouldSearch, requestUrl) = track.Url.ToLower() switch {
            var yt when yt.Contains("youtube")
                => (true, $"https://img.youtube.com/vi/{track.Id}/maxresdefault.jpg"),

            var twitch when twitch.Contains("twitch")
                => (true, $"https://api.twitch.tv/v4/oembed?url={track.Url}"),

            var sc when sc.Contains("soundcloud")
                => (true, $"https://soundcloud.com/oembed?url={track.Url}&format=json"),

            var vim when vim.Contains("vimeo")
                => (false, $"https://i.vimeocdn.com/video/{track.Id}.png"),

            _ => (false, "https://raw.githubusercontent.com/Yucked/Victoria/v5/src/Logo.png")
        };

        if (!shouldSearch) {
            return requestUrl;
        }

        var (httpMethod, httpCompletionOption, fallbackUrl) = track.Url.ToLower().Contains("youtube")
            ? (HttpMethod.Head, HttpCompletionOption.ResponseHeadersRead,
               $"https://img.youtube.com/vi/{track.Id}/hqdefault.jpg")
            : (HttpMethod.Get, HttpCompletionOption.ResponseContentRead, null);

        var responseMessage = await _httpClient.SendAsync(new HttpRequestMessage {
            Method = httpMethod,
            RequestUri = new Uri(requestUrl)
        }, httpCompletionOption);

        if (!responseMessage.IsSuccessStatusCode) {
            return fallbackUrl ?? throw new Exception(responseMessage.ReasonPhrase);
        }
        else if (track.Url.ToLower().Contains("youtube")) {
            return requestUrl;
        }

        using var content = responseMessage.Content;
        await using var stream = await content.ReadAsStreamAsync();

        var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.TryGetProperty("thumbnail_url", out var url)
            ? $"{url}"
            : requestUrl;
    }

    private static void GetRequestInformation(string url) {
        var _ = url.ToLower() switch {
            var yt when yt.Contains("youtube")
                => (HttpMethod.Head)
        };
    }
}