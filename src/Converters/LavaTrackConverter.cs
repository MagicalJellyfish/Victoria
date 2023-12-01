using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victoria.Converters;

internal sealed class LavaTrackConverter : JsonConverter<LavaTrack> {
    public override LavaTrack Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        string hash = null,
               id = null,
               title = null,
               author = null,
               url = default,
               source = default,
               artworkUrl = default,
               isrc = default;

        long length = default,
             position = default;

        bool isSeekable = false,
             isLiveStream = false;

        while (reader.Read()) {
            if (reader.TokenType is not (JsonTokenType.StartObject and JsonTokenType.EndObject)) {
                continue;
            }

            if (reader.TokenType == JsonTokenType.PropertyName
                && reader.ValueTextEquals("encoded")
                && reader.Read()) {
                hash = reader.GetString();
            }

            if (IsProp(ref reader, "identifier")) {
                id = reader.GetString();
            }
            else if (IsProp(ref reader, "title")) {
                title = reader.GetString();
            }
            else if (IsProp(ref reader, "author")) {
                author = reader.GetString();
            }
            else if (IsProp(ref reader, "uri")) {
                url = reader.GetString();
            }
            else if (IsProp(ref reader, "sourceName")) {
                source = reader.GetString();
            }
            else if (IsProp(ref reader, "artworkUrl")) {
                artworkUrl = reader.GetString();
            }
            else if (IsProp(ref reader, "isrc")) {
                isrc = reader.GetString();
            }
            else if (IsProp(ref reader, "isSeekable")) {
                isSeekable = reader.GetBoolean();
            }
            else if (IsProp(ref reader, "isStream")) {
                isLiveStream = reader.GetBoolean();
            }
            else if (IsProp(ref reader, "length")) {
                length = reader.GetInt64();
            }
            else if (IsProp(ref reader, "position")) {
                position = reader.GetInt64();
            }
        }

        var lavaTrack = new LavaTrack {
            Hash = hash,
            Id = id,
            Title = title,
            Author = author,
            Artwork = artworkUrl,
            ISRC = isrc,
            SourceName = source,
            IsSeekable = isSeekable,
            IsLiveStream = isLiveStream,
            Position = TimeSpan.FromMilliseconds(position),
            Duration = TimeSpan.FromMilliseconds(length)
        };

        return lavaTrack;
    }

    public override void Write(Utf8JsonWriter writer, LavaTrack value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }

    private bool IsProp(ref Utf8JsonReader reader, string text) {
        return reader.ValueTextEquals(text) && reader.Read();
    }
}