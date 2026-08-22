using System.Text.Json;
using Shouldly;
using Whoop.Sdk.Models;
using Whoop.Sdk.Serialization;
using Xunit;

namespace Whoop.Sdk.Tests.Serialization;

public sealed class ScoreStateConverterTests
{
    [Theory]
    [InlineData("\"SCORED\"", ScoreState.Scored)]
    [InlineData("\"PENDING_SCORE\"", ScoreState.PendingScore)]
    [InlineData("\"UNSCORABLE\"", ScoreState.Unscorable)]
    public void Reads_every_documented_state(string json, ScoreState expected) =>
        JsonSerializer.Deserialize<ScoreState>(json, WhoopJson.Options).ShouldBe(expected);

    [Fact]
    public void Degrades_an_unrecognised_state_to_Unknown_rather_than_throwing() =>
        JsonSerializer.Deserialize<ScoreState>("\"SOMETHING_NEW\"", WhoopJson.Options).ShouldBe(ScoreState.Unknown);

    [Fact]
    public void Reads_null_as_Unknown() =>
        JsonSerializer.Deserialize<ScoreState>("null", WhoopJson.Options).ShouldBe(ScoreState.Unknown);

    [Fact]
    public void Rejects_a_non_string_token() =>
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ScoreState>("7", WhoopJson.Options));

    [Theory]
    [InlineData(ScoreState.Scored, "\"SCORED\"")]
    [InlineData(ScoreState.PendingScore, "\"PENDING_SCORE\"")]
    [InlineData(ScoreState.Unscorable, "\"UNSCORABLE\"")]
    [InlineData(ScoreState.Unknown, "null")]
    public void Writes_the_wire_format(ScoreState state, string expected) =>
        JsonSerializer.Serialize(state, WhoopJson.Options).ShouldBe(expected);
}
