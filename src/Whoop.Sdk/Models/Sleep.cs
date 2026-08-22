using System;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Models
{
    /// <summary>A sleep or nap activity recorded by WHOOP.</summary>
    public sealed record Sleep
    {
        /// <summary>Unique identifier for the sleep activity.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        /// <summary>The cycle this sleep belongs to.</summary>
        [JsonPropertyName("cycle_id")]
        public long CycleId { get; init; }

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

        /// <summary>When the sleep started.</summary>
        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; init; }

        /// <summary>When the sleep ended.</summary>
        [JsonPropertyName("end")]
        public DateTimeOffset End { get; init; }

        /// <summary>The user's UTC offset at the time of the sleep, formatted as <c>+hh:mm</c>.</summary>
        [JsonPropertyName("timezone_offset")]
        public string? TimezoneOffset { get; init; }

        /// <summary><see langword="true"/> when the activity was a nap rather than a full night's sleep.</summary>
        [JsonPropertyName("nap")]
        public bool Nap { get; init; }

        /// <summary>Whether <see cref="Score"/> has been calculated.</summary>
        [JsonPropertyName("score_state")]
        public ScoreState ScoreState { get; init; }

        /// <summary>The sleep score; present only when <see cref="ScoreState"/> is <see cref="ScoreState.Scored"/>.</summary>
        [JsonPropertyName("score")]
        public SleepScore? Score { get; init; }

        /// <summary>Wall-clock duration between <see cref="Start"/> and <see cref="End"/>.</summary>
        [JsonIgnore]
        public TimeSpan Duration => End - Start;
    }

    /// <summary>Scored sleep metrics.</summary>
    public sealed record SleepScore
    {
        /// <summary>Time spent in each sleep stage.</summary>
        [JsonPropertyName("stage_summary")]
        public SleepStageSummary? StageSummary { get; init; }

        /// <summary>How much sleep the user needed, broken down by contributing factor.</summary>
        [JsonPropertyName("sleep_needed")]
        public SleepNeeded? SleepNeeded { get; init; }

        /// <summary>Respiratory rate during sleep, in breaths per minute.</summary>
        [JsonPropertyName("respiratory_rate")]
        public float? RespiratoryRate { get; init; }

        /// <summary>Percentage of needed sleep that the user actually got.</summary>
        [JsonPropertyName("sleep_performance_percentage")]
        public float? SleepPerformancePercentage { get; init; }

        /// <summary>How consistent the user's sleep and wake times were, as a percentage.</summary>
        [JsonPropertyName("sleep_consistency_percentage")]
        public float? SleepConsistencyPercentage { get; init; }

        /// <summary>Percentage of time in bed that was actually spent asleep.</summary>
        [JsonPropertyName("sleep_efficiency_percentage")]
        public float? SleepEfficiencyPercentage { get; init; }
    }

    /// <summary>Time spent in each sleep stage, reported by the API in milliseconds.</summary>
    public sealed record SleepStageSummary
    {
        /// <summary>Total time in bed, in milliseconds.</summary>
        [JsonPropertyName("total_in_bed_time_milli")]
        public int TotalInBedTimeMilli { get; init; }

        /// <summary>Total time awake while in bed, in milliseconds.</summary>
        [JsonPropertyName("total_awake_time_milli")]
        public int TotalAwakeTimeMilli { get; init; }

        /// <summary>Total time for which no data was captured, in milliseconds.</summary>
        [JsonPropertyName("total_no_data_time_milli")]
        public int TotalNoDataTimeMilli { get; init; }

        /// <summary>Total light sleep time, in milliseconds.</summary>
        [JsonPropertyName("total_light_sleep_time_milli")]
        public int TotalLightSleepTimeMilli { get; init; }

        /// <summary>Total slow wave (deep) sleep time, in milliseconds.</summary>
        [JsonPropertyName("total_slow_wave_sleep_time_milli")]
        public int TotalSlowWaveSleepTimeMilli { get; init; }

        /// <summary>Total REM sleep time, in milliseconds.</summary>
        [JsonPropertyName("total_rem_sleep_time_milli")]
        public int TotalRemSleepTimeMilli { get; init; }

        /// <summary>Number of sleep cycles completed.</summary>
        [JsonPropertyName("sleep_cycle_count")]
        public int SleepCycleCount { get; init; }

        /// <summary>Number of times the user was disturbed during sleep.</summary>
        [JsonPropertyName("disturbance_count")]
        public int DisturbanceCount { get; init; }

        /// <summary><see cref="TotalInBedTimeMilli"/> as a <see cref="TimeSpan"/>.</summary>
        [JsonIgnore]
        public TimeSpan TotalInBedTime => TimeSpan.FromMilliseconds(TotalInBedTimeMilli);

        /// <summary><see cref="TotalAwakeTimeMilli"/> as a <see cref="TimeSpan"/>.</summary>
        [JsonIgnore]
        public TimeSpan TotalAwakeTime => TimeSpan.FromMilliseconds(TotalAwakeTimeMilli);

        /// <summary><see cref="TotalLightSleepTimeMilli"/> as a <see cref="TimeSpan"/>.</summary>
        [JsonIgnore]
        public TimeSpan TotalLightSleepTime => TimeSpan.FromMilliseconds(TotalLightSleepTimeMilli);

        /// <summary><see cref="TotalSlowWaveSleepTimeMilli"/> as a <see cref="TimeSpan"/>.</summary>
        [JsonIgnore]
        public TimeSpan TotalSlowWaveSleepTime => TimeSpan.FromMilliseconds(TotalSlowWaveSleepTimeMilli);

        /// <summary><see cref="TotalRemSleepTimeMilli"/> as a <see cref="TimeSpan"/>.</summary>
        [JsonIgnore]
        public TimeSpan TotalRemSleepTime => TimeSpan.FromMilliseconds(TotalRemSleepTimeMilli);

        /// <summary>Time actually asleep: time in bed minus awake and no-data time.</summary>
        [JsonIgnore]
        public TimeSpan TotalAsleepTime => TimeSpan.FromMilliseconds(
            TotalInBedTimeMilli - TotalAwakeTimeMilli - TotalNoDataTimeMilli);
    }

    /// <summary>Breakdown of how much sleep the user needed.</summary>
    public sealed record SleepNeeded
    {
        /// <summary>Baseline sleep need, in milliseconds.</summary>
        [JsonPropertyName("baseline_milli")]
        public long BaselineMilli { get; init; }

        /// <summary>Extra sleep needed to repay accumulated sleep debt, in milliseconds.</summary>
        [JsonPropertyName("need_from_sleep_debt_milli")]
        public long NeedFromSleepDebtMilli { get; init; }

        /// <summary>Extra sleep needed because of recent strain, in milliseconds.</summary>
        [JsonPropertyName("need_from_recent_strain_milli")]
        public long NeedFromRecentStrainMilli { get; init; }

        /// <summary>Reduction in sleep need earned by recent naps, in milliseconds. Typically negative.</summary>
        [JsonPropertyName("need_from_recent_nap_milli")]
        public long NeedFromRecentNapMilli { get; init; }

        /// <summary>Total sleep need: the sum of all contributing factors.</summary>
        [JsonIgnore]
        public TimeSpan Total => TimeSpan.FromMilliseconds(
            BaselineMilli + NeedFromSleepDebtMilli + NeedFromRecentStrainMilli + NeedFromRecentNapMilli);
    }
}
