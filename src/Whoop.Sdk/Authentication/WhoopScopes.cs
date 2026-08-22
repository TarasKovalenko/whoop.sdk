namespace Whoop.Sdk.Authentication
{
    /// <summary>The OAuth scopes exposed by the WHOOP developer platform.</summary>
    public static class WhoopScopes
    {
        /// <summary>Read cycle data, including day strain and average heart rate.</summary>
        public const string ReadCycles = "read:cycles";

        /// <summary>Read recovery data, including score, heart rate variability, and resting heart rate.</summary>
        public const string ReadRecovery = "read:recovery";

        /// <summary>Read sleep data, including performance percentage and per-stage duration.</summary>
        public const string ReadSleep = "read:sleep";

        /// <summary>Read workout data, including activity strain and average heart rate.</summary>
        public const string ReadWorkout = "read:workout";

        /// <summary>Read profile data, including name and email.</summary>
        public const string ReadProfile = "read:profile";

        /// <summary>Read body measurements, including height, weight, and max heart rate.</summary>
        public const string ReadBodyMeasurement = "read:body_measurement";

        /// <summary>Request a refresh token so access can continue without the user being present.</summary>
        public const string Offline = "offline";

        /// <summary>Scope required by the trusted-partner client credentials flow.</summary>
        public const string PartnerToken = "whoop-partner/token";

        /// <summary>Every read scope, for callers that want the full data set.</summary>
        public static string[] AllRead() => new[]
        {
            ReadCycles,
            ReadRecovery,
            ReadSleep,
            ReadWorkout,
            ReadProfile,
            ReadBodyMeasurement,
        };
    }
}
