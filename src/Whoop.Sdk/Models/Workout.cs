using System;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Models
{
    /// <summary>A workout activity recorded by WHOOP (the v2 <c>WorkoutV2</c> schema).</summary>
    public sealed record Workout
    {
        /// <summary>Unique identifier for the workout.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        /// <summary>The identifier this activity had in the deprecated v1 API, when it has one.</summary>
        [JsonPropertyName("v1_id")]
        public long? V1Id { get; init; }

        /// <summary>The WHOOP user who owns the activity.</summary>
        [JsonPropertyName("user_id")]
        public long UserId { get; init; }

        /// <summary>When the record was first created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>When the record was last updated.</summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>When the workout started.</summary>
        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; init; }

        /// <summary>When the workout ended.</summary>
        [JsonPropertyName("end")]
        public DateTimeOffset End { get; init; }

        /// <summary>The user's UTC offset at the time of the workout, formatted as <c>+hh:mm</c>.</summary>
        [JsonPropertyName("timezone_offset")]
        public string? TimezoneOffset { get; init; }

        /// <summary>Name of the sport, for example <c>running</c>.</summary>
        [JsonPropertyName("sport_name")]
        public string? SportName { get; init; }

        /// <summary>Numeric sport identifier from the deprecated v1 API. Prefer <see cref="SportName"/>.</summary>
        [JsonPropertyName("sport_id")]
        public int? SportId { get; init; }

        /// <summary>Whether <see cref="Score"/> has been calculated.</summary>
        [JsonPropertyName("score_state")]
        public ScoreState ScoreState { get; init; }

        /// <summary>The workout score; present only when <see cref="ScoreState"/> is <see cref="ScoreState.Scored"/>.</summary>
        [JsonPropertyName("score")]
        public WorkoutScore? Score { get; init; }

        /// <summary>Wall-clock duration between <see cref="Start"/> and <see cref="End"/>.</summary>
        [JsonIgnore]
        public TimeSpan Duration => End - Start;
    }

    /// <summary>Scored workout metrics.</summary>
    public sealed record WorkoutScore
    {
        /// <summary>Workout strain on WHOOP's 0-21 logarithmic scale.</summary>
        [JsonPropertyName("strain")]
        public float Strain { get; init; }

        /// <summary>Average heart rate during the workout, in beats per minute.</summary>
        [JsonPropertyName("average_heart_rate")]
        public int AverageHeartRate { get; init; }

        /// <summary>Maximum heart rate during the workout, in beats per minute.</summary>
        [JsonPropertyName("max_heart_rate")]
        public int MaxHeartRate { get; init; }

        /// <summary>Energy expended during the workout, in kilojoules.</summary>
        [JsonPropertyName("kilojoule")]
        public float Kilojoule { get; init; }

        /// <summary>Percentage of the workout for which heart rate data was captured.</summary>
        [JsonPropertyName("percent_recorded")]
        public float PercentRecorded { get; init; }

        /// <summary>Distance covered, in metres. Only present when the workout was GPS tracked.</summary>
        [JsonPropertyName("distance_meter")]
        public float? DistanceMeter { get; init; }

        /// <summary>Cumulative altitude gained, in metres.</summary>
        [JsonPropertyName("altitude_gain_meter")]
        public float? AltitudeGainMeter { get; init; }

        /// <summary>Net altitude change between start and end, in metres.</summary>
        [JsonPropertyName("altitude_change_meter")]
        public float? AltitudeChangeMeter { get; init; }

        /// <summary>Time spent in each heart rate zone.</summary>
        [JsonPropertyName("zone_durations")]
        public ZoneDurations? ZoneDurations { get; init; }

        /// <summary>Energy expended during the workout, converted to calories (kcal).</summary>
        [JsonIgnore]
        public double Calories => Kilojoule / 4.184d;
    }

    /// <summary>Time spent in each WHOOP heart rate zone, reported by the API in milliseconds.</summary>
    public sealed record ZoneDurations
    {
        /// <summary>Time below zone one (under 50% of max heart rate), in milliseconds.</summary>
        [JsonPropertyName("zone_zero_milli")]
        public long ZoneZeroMilli { get; init; }

        /// <summary>Time in zone one (50-60% of max heart rate), in milliseconds.</summary>
        [JsonPropertyName("zone_one_milli")]
        public long ZoneOneMilli { get; init; }

        /// <summary>Time in zone two (60-70% of max heart rate), in milliseconds.</summary>
        [JsonPropertyName("zone_two_milli")]
        public long ZoneTwoMilli { get; init; }

        /// <summary>Time in zone three (70-80% of max heart rate), in milliseconds.</summary>
        [JsonPropertyName("zone_three_milli")]
        public long ZoneThreeMilli { get; init; }

        /// <summary>Time in zone four (80-90% of max heart rate), in milliseconds.</summary>
        [JsonPropertyName("zone_four_milli")]
        public long ZoneFourMilli { get; init; }

        /// <summary>Time in zone five (90-100% of max heart rate), in milliseconds.</summary>
        [JsonPropertyName("zone_five_milli")]
        public long ZoneFiveMilli { get; init; }

        /// <summary>Total time across all zones.</summary>
        [JsonIgnore]
        public TimeSpan Total => TimeSpan.FromMilliseconds(
            ZoneZeroMilli + ZoneOneMilli + ZoneTwoMilli + ZoneThreeMilli + ZoneFourMilli + ZoneFiveMilli);
    }
}
