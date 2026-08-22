using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Whoop.Sdk.Tests.TestSupport;

/// <summary>
/// A fake transport that records every request and replays canned responses in order. The last
/// queued response is reused once the queue is exhausted, which keeps paging tests readable.
/// </summary>
public sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly ConcurrentQueue<HttpResponseMessage> _responses = new();
    private readonly List<RecordedRequest> _requests = new();
    private readonly object _sync = new();

    private HttpResponseMessage? _lastResponse;

    public IReadOnlyList<RecordedRequest> Requests
    {
        get
        {
            lock (_sync)
            {
                return _requests.ToArray();
            }
        }
    }

    public RecordedRequest LastRequest => Requests[^1];

    public RecordingHttpMessageHandler RespondWith(HttpResponseMessage response)
    {
        _responses.Enqueue(response);
        return this;
    }

    public RecordingHttpMessageHandler RespondWithJson(string json, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        RespondWith(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });

    public RecordingHttpMessageHandler RespondWithStatus(HttpStatusCode statusCode, string? body = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (body is not null)
        {
            response.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return RespondWith(response);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var headers = request.Headers
            .ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase);

        lock (_sync)
        {
            _requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri!,
                body,
                request.Content?.Headers.ContentType?.MediaType,
                headers));
        }

        if (_responses.TryDequeue(out var response))
        {
            _lastResponse = response;
            response.RequestMessage = request;
            return response;
        }

        if (_lastResponse is not null)
        {
            var replay = Clone(_lastResponse);
            replay.RequestMessage = request;
            return replay;
        }

        return new HttpResponseMessage(HttpStatusCode.NoContent) { RequestMessage = request };
    }

    private static HttpResponseMessage Clone(HttpResponseMessage source)
    {
        var clone = new HttpResponseMessage(source.StatusCode);
        if (source.Content is not null)
        {
            var payload = source.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            clone.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        }

        return clone;
    }
}
