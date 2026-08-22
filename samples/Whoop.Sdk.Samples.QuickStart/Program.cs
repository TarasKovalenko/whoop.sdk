// Reads a WHOOP account with a single access token and no dependency injection.
//
//   export WHOOP_ACCESS_TOKEN="..."
//   dotnet run --project samples/Whoop.Sdk.Samples.QuickStart
//
// Get a token from https://developer-dashboard.whoop.com, or run the OAuth sample to mint one.

using Whoop.Sdk;
using Whoop.Sdk.Models;

var accessToken = Environment.GetEnvironmentVariable("WHOOP_ACCESS_TOKEN");
if (string.IsNullOrWhiteSpace(accessToken))
{
    Console.Error.WriteLine("Set WHOOP_ACCESS_TOKEN first.");
    return 1;
}

using var whoop = new WhoopClient(accessToken);

try
{
    var profile = await whoop.User.GetBasicProfileAsync();
    var body = await whoop.User.GetBodyMeasurementAsync();

    Console.WriteLine($"{profile.FirstName} {profile.LastName} <{profile.Email}>");
    Console.WriteLine($"{body.HeightMeter:F2} m, {body.WeightKilogram:F1} kg, max HR {body.MaxHeartRate}");
    Console.WriteLine();

    await PrintTodayAsync(whoop);
    await PrintRecentRecoveryAsync(whoop);
    await PrintWorkoutTotalsAsync(whoop);

    return 0;
}
catch (WhoopRateLimitExceededException exception)
{
    // WHOOP allows 100 requests/minute and 10,000/day per user.
    Console.Error.WriteLine($"Rate limited. Retry after {exception.RetryAfter ?? TimeSpan.FromSeconds(30)}.");
    return 2;
}
catch (WhoopApiException exception) when (exception.IsAuthenticationFailure)
{
    Console.Error.WriteLine("The token is expired or missing a scope. WHOOP tokens last about an hour.");
    return 3;
}
catch (WhoopApiException exception)
{
    Console.Error.WriteLine($"{exception.Message}\n{exception.ResponseBody}");
    return 4;
}

// The most recent cycle is today's; it stays unscored until it ends.
static async Task PrintTodayAsync(IWhoopClient whoop)
{
    var page = await whoop.Cycles.ListAsync(new WhoopCollectionRequest { Limit = 1 });
    if (page.Records.Count == 0)
    {
        Console.WriteLine("No cycles yet.");
        return;
    }

    var cycle = page.Records[0];
    Console.WriteLine($"Current cycle {cycle.Id} started {cycle.Start:g} ({(cycle.IsInProgress ? "in progress" : "closed")})");

    switch (cycle.ScoreState)
    {
        case ScoreState.Scored:
            Console.WriteLine($"  strain {cycle.Score!.Strain:F1}, {cycle.Score.Calories:F0} kcal, avg HR {cycle.Score.AverageHeartRate}");
            break;
        case ScoreState.PendingScore:
            Console.WriteLine("  not scored yet - check back later");
            break;
        case ScoreState.Unscorable:
            Console.WriteLine("  too little data to score");
            break;
        default:
            Console.WriteLine("  unrecognised score state");
            break;
    }

    // Sleep and recovery hang off the cycle.
    try
    {
        var recovery = await whoop.Cycles.GetRecoveryAsync(cycle.Id);
        if (recovery.Score is { } score)
        {
            Console.WriteLine($"  recovery {score.RecoveryScorePercentage:F0}%, HRV {score.HrvRmssdMilli:F0} ms, RHR {score.RestingHeartRate:F0}");
        }
    }
    catch (WhoopApiException exception) when (exception.IsNotFound)
    {
        Console.WriteLine("  no recovery for this cycle yet");
    }

    Console.WriteLine();
}

static async Task PrintRecentRecoveryAsync(IWhoopClient whoop)
{
    Console.WriteLine("Last 7 days of recovery:");

    var request = new WhoopCollectionRequest
    {
        Start = DateTimeOffset.UtcNow.AddDays(-7),
        Limit = 25,
    };

    await foreach (var recovery in whoop.Recovery.EnumerateAsync(request))
    {
        // Only Scored guarantees a Score; calibrating users have placeholder numbers.
        if (recovery.ScoreState != ScoreState.Scored || recovery.Score!.UserCalibrating)
        {
            continue;
        }

        Console.WriteLine($"  {recovery.CreatedAt:yyyy-MM-dd}  {recovery.Score.RecoveryScorePercentage,5:F0}%  HRV {recovery.Score.HrvRmssdMilli,5:F0} ms");
    }

    Console.WriteLine();
}

// EnumerateAsync follows the next_token cursor and only fetches a page when one is consumed.
static async Task PrintWorkoutTotalsAsync(IWhoopClient whoop)
{
    var request = new WhoopCollectionRequest
    {
        Start = DateTimeOffset.UtcNow.AddDays(-30),
        Limit = 25,
    };

    var totals = new Dictionary<string, (int Count, TimeSpan Duration, double Strain)>(StringComparer.OrdinalIgnoreCase);

    await foreach (var workout in whoop.Workouts.EnumerateAsync(request))
    {
        var sport = workout.SportName ?? "unknown";
        var current = totals.TryGetValue(sport, out var existing) ? existing : default;

        totals[sport] = (
            current.Count + 1,
            current.Duration + workout.Duration,
            current.Strain + (workout.Score?.Strain ?? 0));
    }

    Console.WriteLine("Last 30 days of workouts:");
    foreach (var (sport, total) in totals.OrderByDescending(entry => entry.Value.Strain))
    {
        Console.WriteLine($"  {sport,-20} {total.Count,3}x  {total.Duration.TotalHours,5:F1} h  strain {total.Strain,6:F1}");
    }
}
