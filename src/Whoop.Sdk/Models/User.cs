using System;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Models
{
    /// <summary>The authenticated user's basic profile.</summary>
    public sealed record UserBasicProfile
    {
        /// <summary>The WHOOP user identifier.</summary>
        [JsonPropertyName("user_id")]
        public long UserId { get; init; }

        /// <summary>The user's email address.</summary>
        [JsonPropertyName("email")]
        public string? Email { get; init; }

        /// <summary>The user's first name.</summary>
        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        /// <summary>The user's last name.</summary>
        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }
    }

    /// <summary>The authenticated user's body measurements.</summary>
    public sealed record UserBodyMeasurement
    {
        /// <summary>Height, in metres.</summary>
        [JsonPropertyName("height_meter")]
        public float HeightMeter { get; init; }

        /// <summary>Weight, in kilograms.</summary>
        [JsonPropertyName("weight_kilogram")]
        public float WeightKilogram { get; init; }

        /// <summary>Maximum heart rate, in beats per minute.</summary>
        [JsonPropertyName("max_heart_rate")]
        public int MaxHeartRate { get; init; }
    }

    /// <summary>Maps a deprecated v1 activity identifier onto its v2 replacement.</summary>
    public sealed record ActivityIdMapping
    {
        /// <summary>The v2 identifier of the activity.</summary>
        [JsonPropertyName("v2_activity_id")]
        public Guid V2ActivityId { get; init; }
    }
}
