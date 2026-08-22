using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Whoop.Sdk.Tests.TestSupport;

/// <summary>An immutable snapshot of a request that reached the fake transport.</summary>
public sealed record RecordedRequest(
    HttpMethod Method,
    Uri RequestUri,
    string? Body,
    string? ContentType,
    IReadOnlyDictionary<string, string> Headers)
{
    public string PathAndQuery => RequestUri.PathAndQuery;

    public string Path => RequestUri.AbsolutePath;

    public string Query => RequestUri.Query;
}
