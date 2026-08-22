using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Whoop.Sdk.Endpoints;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;
using Whoop.Sdk.Tests.TestSupport;
using Xunit;

namespace Whoop.Sdk.Tests.Endpoints;

public sealed class PaginationTests
{
    [Fact]
    public async Task Enumerate_walks_every_page_until_the_cursor_runs_out()
    {
        var handler = new RecordingHttpMessageHandler()
            .RespondWithJson(Page(1, "cursor-1"))
            .RespondWithJson(Page(2, "cursor-2"))
            .RespondWithJson(Page(3, nextToken: null));

        var client = new CycleClient(CreateConnection(handler));

        var ids = new List<long>();
        await foreach (var cycle in client.EnumerateAsync())
        {
            ids.Add(cycle.Id);
        }

        ids.ShouldBe(new long[] { 1, 2, 3 });
        handler.Requests.Count.ShouldBe(3);
        handler.Requests[0].Query.ShouldBeEmpty();
        handler.Requests[1].Query.ShouldBe("?nextToken=cursor-1");
        handler.Requests[2].Query.ShouldBe("?nextToken=cursor-2");
    }

    [Fact]
    public async Task Enumerate_carries_the_filters_onto_every_page()
    {
        var handler = new RecordingHttpMessageHandler()
            .RespondWithJson(Page(1, "cursor-1"))
            .RespondWithJson(Page(2, nextToken: null));

        var client = new CycleClient(CreateConnection(handler));
        var request = new WhoopCollectionRequest
        {
            Limit = 25,
            Start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };

        await client.EnumerateAsync(request).ToListAsync();

        handler.Requests[1].Query.ShouldBe("?limit=25&start=2024-01-01T00%3A00%3A00.000Z&nextToken=cursor-1");
    }

    [Fact]
    public async Task Enumerate_stops_when_the_server_repeats_a_cursor()
    {
        var handler = new RecordingHttpMessageHandler()
            .RespondWithJson(Page(1, "stuck"))
            .RespondWithJson(Page(2, "stuck"));

        var client = new CycleClient(CreateConnection(handler));

        var cycles = await client.EnumerateAsync().ToListAsync();

        cycles.Count.ShouldBe(2);
        handler.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Enumerate_treats_an_empty_cursor_as_the_end()
    {
        var handler = new RecordingHttpMessageHandler().RespondWithJson(Page(1, string.Empty));

        var client = new CycleClient(CreateConnection(handler));

        (await client.EnumerateAsync().ToListAsync()).Count.ShouldBe(1);
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Enumerate_is_lazy_and_only_fetches_what_is_consumed()
    {
        var handler = new RecordingHttpMessageHandler()
            .RespondWithJson(Page(1, "cursor-1"))
            .RespondWithJson(Page(2, "cursor-2"));

        var client = new CycleClient(CreateConnection(handler));

        await foreach (var _ in client.EnumerateAsync())
        {
            break;
        }

        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Enumerate_honours_cancellation()
    {
        var handler = new RecordingHttpMessageHandler().RespondWithJson(Page(1, "cursor-1"));
        var client = new CycleClient(CreateConnection(handler));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in client.EnumerateAsync(cancellationToken: cts.Token))
            {
                // The first MoveNext should already observe the cancellation.
            }
        });
    }

    [Fact]
    public void Limit_is_validated_against_the_documented_maximum()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new WhoopCollectionRequest { Limit = 26 });
        Should.Throw<ArgumentOutOfRangeException>(() => new WhoopCollectionRequest { Limit = 0 });
        new WhoopCollectionRequest { Limit = WhoopCollectionRequest.MaxLimit }.Limit.ShouldBe(25);
    }

    private static string Page(long id, string? nextToken)
    {
        var token = nextToken is null ? "null" : $"\"{nextToken}\"";
        return $$"""
            {
              "records": [
                {
                  "id": {{id}},
                  "user_id": 1,
                  "created_at": "2024-01-01T00:00:00.000Z",
                  "updated_at": "2024-01-01T00:00:00.000Z",
                  "start": "2024-01-01T00:00:00.000Z",
                  "timezone_offset": "+00:00",
                  "score_state": "SCORED"
                }
              ],
              "next_token": {{token}}
            }
            """;
    }

    private static WhoopApiConnection CreateConnection(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://api.prod.whoop.com/developer/") });
}

internal static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var items = new List<T>();
        await foreach (var item in source)
        {
            items.Add(item);
        }

        return items;
    }
}
