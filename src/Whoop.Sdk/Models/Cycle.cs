using System;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Models
{
    /// <summary>A WHOOP physiological cycle: the period between the start of one sleep and the next.</summary>
    public sealed record Cycle
    {
        /// <summary>Unique identifier for the cycle.</summary>
        [JsonPropertyName("id")]
        public long Id { get; init; }

        /// <summary>The WHOOP user who owns the cycle.</summary>
        [JsonPropertyName("user_id")]
        public long UserId { get; init; }

        /// <summary>When the record was first created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>When the record was last updated.</summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>When the cycle started.</summary>
        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; init; }

        /// <summary>When the cycle ended, or <see langword="null"/> while it is still in progress.</summary>
        [JsonPropertyName("end")]
        public DateTimeOffset? End { get; init; }

        /// <summary>The user's UTC offset at the time of the cycle, formatted as <c>+hh:mm</c>.</summary>
        [JsonPropertyName("timezone_offset")]
        public string? TimezoneOffset { get; init; }

        /// <summary>Whether <see cref="Score"/> has been calculated.</summary>
        [JsonPropertyName("score_state")]
        public ScoreState ScoreState { get; init; }

        /// <summary>The cycle score; present only when <see cref="ScoreState"/> is <see cref="ScoreState.Scored"/>.</summary>
        [JsonPropertyName("score")]
        public CycleScore? Score { get; init; }

        /// <summary><see langword="true"/> while the cycle has not ended yet.</summary>
        [JsonIgnore]
        public bool IsInProgress => End is null;
    }

    /// <summary>Strain and heart-rate summary for a <see cref="Cycle"/>.</summary>
    public sealed record CycleScore
    {
        /// <summary>Day strain on WHOOP's 0-21 logarithmic scale.</summary>
        [JsonPropertyName("strain")]
        public float Strain { get; init; }

        /// <summary>Energy expended during the cycle, in kilojoules.</summary>
        [JsonPropertyName("kilojoule")]
        public float Kilojoule { get; init; }

        /// <summary>Average heart rate during the cycle, in beats per minute.</summary>
        [JsonPropertyName("average_heart_rate")]
        public int AverageHeartRate { get; init; }

        /// <summary>Maximum heart rate during the cycle, in beats per minute.</summary>
        [JsonPropertyName("max_heart_rate")]
        public int MaxHeartRate { get; init; }

        /// <summary>Energy expended during the cycle, converted to calories (kcal).</summary>
        [JsonIgnore]
        public double Calories => Kilojoule / 4.184d;
    }
}
