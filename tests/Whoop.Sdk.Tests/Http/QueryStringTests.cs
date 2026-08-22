using System;
using System.Collections.Generic;
using Shouldly;
using Whoop.Sdk.Http;
using Xunit;

namespace Whoop.Sdk.Tests.Http;

public sealed class QueryStringTests
{
    [Fact]
    public void Returns_an_empty_string_when_there_is_nothing_to_send() =>
        QueryString.Build(null).ShouldBeEmpty();

    [Fact]
    public void Returns_an_empty_string_when_every_value_is_null()
    {
        var parameters = new[] { new KeyValuePair<string, string?>("limit", null) };

        QueryString.Build(parameters).ShouldBeEmpty();
    }

    [Fact]
    public void Preserves_the_order_it_was_given()
    {
        var parameters = new[]
        {
            new KeyValuePair<string, string?>("limit", "10"),
            new KeyValuePair<string, string?>("start", "2024-01-01T00:00:00.000Z"),
        };

        QueryString.Build(parameters).ShouldBe("?limit=10&start=2024-01-01T00%3A00%3A00.000Z");
    }

    [Fact]
    public void Escapes_keys_and_values()
    {
        var parameters = new[] { new KeyValuePair<string, string?>("next token", "a b&c=d") };

        QueryString.Build(parameters).ShouldBe("?next%20token=a%20b%26c%3Dd");
    }

    [Fact]
    public void Formats_instants_as_utc_with_millisecond_precision()
    {
        var instant = new DateTimeOffset(2024, 3, 1, 15, 4, 5, 678, TimeSpan.FromHours(2));

        QueryString.Format(instant).ShouldBe("2024-03-01T13:04:05.678Z");
    }

    [Fact]
    public void Formats_a_null_instant_as_null() =>
        QueryString.Format((DateTimeOffset?)null).ShouldBeNull();

    [Fact]
    public void Formats_integers_invariantly() =>
        QueryString.Format(25).ShouldBe("25");
}
