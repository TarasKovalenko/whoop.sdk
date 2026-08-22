using System.Text.Json;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Serialization
{
    /// <summary>Serializer settings used for every WHOOP request and response.</summary>
    public static class WhoopJson
    {
        /// <summary>
        /// The shared, immutable <see cref="JsonSerializerOptions"/> instance. Property names are mapped
        /// with explicit <see cref="JsonPropertyNameAttribute"/> declarations rather than a naming policy,
        /// so the wire format stays stable regardless of the host framework.
        /// </summary>
        public static JsonSerializerOptions Options { get; } = Create();

        private static JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = false,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            options.Converters.Add(new ScoreStateConverter());
            return options;
        }
    }
}
