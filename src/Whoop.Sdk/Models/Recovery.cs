using System;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Models
{
    /// <summary>A recovery score computed for a cycle, derived from the preceding sleep.</summary>
    public sealed record Recovery
    {
        /// <summary>The cycle this recovery belongs to.</summary>
        [JsonPropertyName("cycle_id")]
        public long CycleId { get; init; }

        /// <summary>The sleep the recovery was derived from.</summary>
        [JsonPropertyName("sleep_id")]
        public Guid SleepId { get; init; }

        /// <summary>The WHOOP user who owns the record.</summary>
        [JsonPropertyName("user_id")]
        public long UserId { get; init; }

        /// <summary>When the record was first created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>When the record was last updated.</summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>Whether <see cref="Score"/> has been calculated.</summary>
        [JsonPropertyName("score_state")]
        public ScoreState ScoreState { get; init; }

        /// <summary>The recovery score; present only when <see cref="ScoreState"/> is <see cref="ScoreState.Scored"/>.</summary>
        [JsonPropertyName("score")]
        public RecoveryScore? Score { get; init; }
    }

    /// <summary>Scored recovery metrics.</summary>
    public sealed record RecoveryScore
    {
        /// <summary>
        /// <see langword="true"/> while WHOOP is still establishing the user's baseline, in which case
        /// the remaining values are not yet meaningful.
        /// </summary>
        [JsonPropertyName("user_calibrating")]
        public bool UserCalibrating { get; init; }

        /// <summary>Recovery percentage, from 0 to 100.</summary>
        [JsonPropertyName("recovery_score")]
        public float RecoveryScorePercentage { get; init; }

        /// <summary>Resting heart rate, in beats per minute.</summary>
        [JsonPropertyName("resting_heart_rate")]
        public float RestingHeartRate { get; init; }

        /// <summary>Heart rate variability (RMSSD), in milliseconds.</summary>
        [JsonPropertyName("hrv_rmssd_milli")]
        public float HrvRmssdMilli { get; init; }

        /// <summary>Blood oxygen saturation, as a percentage. Only available on supported hardware.</summary>
        [JsonPropertyName("spo2_percentage")]
        public float? Spo2Percentage { get; init; }

        /// <summary>Skin temperature, in degrees Celsius. Only available on supported hardware.</summary>
        [JsonPropertyName("skin_temp_celsius")]
        public float? SkinTempCelsius { get; init; }
    }
}
