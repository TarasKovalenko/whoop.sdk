using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Samples.Worker;

/// <summary>
/// A background service that pulls yesterday's numbers on a timer. <see cref="IWhoopClient"/> is
/// injected like any other service; the token provider and HTTP pipeline are wired in Program.cs.
/// </summary>
public sealed class DailySummaryService(IWhoopClient whoop, ILogger<DailySummaryService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                await ReportAsync(stoppingToken);
            }
            catch (WhoopRateLimitExceededException exception)
            {
                logger.LogWarning("Rate limited; backing off for {RetryAfter}.", exception.RetryAfter);
                await Task.Delay(exception.RetryAfter ?? TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (WhoopApiException exception) when (exception.IsAuthenticationFailure)
            {
                logger.LogError(exception, "WHOOP rejected the credentials; stopping.");
                return;
            }
            catch (WhoopApiException exception)
            {
                logger.LogError(exception, "WHOOP call failed with {StatusCode}.", exception.StatusCode);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ReportAsync(CancellationToken cancellationToken)
    {
        var profile = await whoop.User.GetBasicProfileAsync(cancellationToken);

        var since = new WhoopCollectionRequest
        {
            Start = DateTimeOffset.UtcNow.AddDays(-1),
            Limit = 25,
        };

        var sleepCount = 0;
        var asleep = TimeSpan.Zero;

        await foreach (var sleep in whoop.Sleep.EnumerateAsync(since, cancellationToken))
        {
            if (sleep.Nap || sleep.ScoreState != ScoreState.Scored)
            {
                continue;
            }

            sleepCount++;
            asleep += sleep.Score!.StageSummary?.TotalAsleepTime ?? TimeSpan.Zero;
        }

        logger.LogInformation(
            "{User}: {SleepCount} sleep(s) in the last 24h totalling {Hours:F1}h.",
            profile.FirstName,
            sleepCount,
            asleep.TotalHours);
    }
}
