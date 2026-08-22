using Whoop.Sdk.Endpoints;

namespace Whoop.Sdk
{
    /// <summary>Entry point to the WHOOP developer API, grouped by resource.</summary>
    public interface IWhoopClient
    {
        /// <summary>Physiological cycles.</summary>
        ICycleClient Cycles { get; }

        /// <summary>Recovery scores.</summary>
        IRecoveryClient Recovery { get; }

        /// <summary>Sleep activities.</summary>
        ISleepClient Sleep { get; }

        /// <summary>Workout activities.</summary>
        IWorkoutClient Workouts { get; }

        /// <summary>Profile and body measurements for the authenticated user.</summary>
        IUserClient User { get; }

        /// <summary>Trusted-partner lab endpoints.</summary>
        IPartnerClient Partner { get; }

        /// <summary>Mapping from deprecated v1 activity identifiers to v2 ones.</summary>
        IActivityMappingClient ActivityMappings { get; }
    }
}
