using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Serialization
{
    /// <summary>
    /// Maps the API's <c>SCREAMING_SNAKE_CASE</c> score states onto <see cref="ScoreState"/>,
    /// degrading unrecognised values to <see cref="ScoreState.Unknown"/> instead of throwing so that
    /// new server-side states never break existing callers.
    /// </summary>
    internal sealed class ScoreStateConverter : JsonConverter<ScoreState>
    {
        public override ScoreState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return ScoreState.Unknown;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected a string value for {nameof(ScoreState)} but found {reader.TokenType}.");
            }

            return reader.GetString() switch
            {
                "SCORED" => ScoreState.Scored,
                "PENDING_SCORE" => ScoreState.PendingScore,
                "UNSCORABLE" => ScoreState.Unscorable,
                _ => ScoreState.Unknown,
            };
        }

        public override void Write(Utf8JsonWriter writer, ScoreState value, JsonSerializerOptions options)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            switch (value)
            {
                case ScoreState.Scored:
                    writer.WriteStringValue("SCORED");
                    break;
                case ScoreState.PendingScore:
                    writer.WriteStringValue("PENDING_SCORE");
                    break;
                case ScoreState.Unscorable:
                    writer.WriteStringValue("UNSCORABLE");
                    break;
                default:
                    writer.WriteNullValue();
                    break;
            }
        }
    }
}
