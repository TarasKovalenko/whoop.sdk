using System;
using System.Text.Json;
using Shouldly;
using Whoop.Sdk.Models;
using Whoop.Sdk.Serialization;
using Whoop.Sdk.Tests.TestSupport;
using Xunit;

namespace Whoop.Sdk.Tests.Serialization;

public sealed class ModelSerializationTests
{
    [Fact]
    public void Reads_a_scored_cycle()
    {
        var cycle = Deserialize<Cycle>(SampleJson.Cycle);

        cycle.Id.ShouldBe(93845);
        cycle.UserId.ShouldBe(10129);
        cycle.Start.ShouldBe(DateTimeOffset.Parse("2022-04-24T02:25:44.774Z", null, System.Globalization.DateTimeStyles.RoundtripKind));
        cycle.End.ShouldNotBeNull();
        cycle.IsInProgress.ShouldBeFalse();
        cycle.TimezoneOffset.ShouldBe("-05:00");
        cycle.ScoreState.ShouldBe(ScoreState.Scored);
        cycle.Score.ShouldNotBeNull();
        cycle.Score!.Strain.ShouldBe(5.2951527f);
        cycle.Score.AverageHeartRate.ShouldBe(68);
        cycle.Score.Calories.ShouldBe(8288.297f / 4.184d, 0.001d);
    }

    [Fact]
    public void Reads_an_in_progress_cycle_without_a_score()
    {
        var page = Deserialize<PaginatedResponse<Cycle>>(SampleJson.CyclePage);

        page.Records.Count.ShouldBe(1);
        page.NextToken.ShouldBe("MTIzOjEyMzEyMwo=");

        var cycle = page.Records[0];
        cycle.End.ShouldBeNull();
        cycle.IsInProgress.ShouldBeTrue();
        cycle.ScoreState.ShouldBe(ScoreState.PendingScore);
        cycle.Score.ShouldBeNull();
    }

    [Fact]
    public void Reads_a_sleep_with_its_stage_summary()
    {
        var sleep = Deserialize<Sleep>(SampleJson.Sleep);

        sleep.Id.ShouldBe(Guid.Parse("ecfc6a15-4661-442f-a9a4-f160dd7afae8"));
        sleep.CycleId.ShouldBe(93845);
        sleep.V1Id.ShouldBe(10235);
        sleep.Nap.ShouldBeFalse();
        sleep.Duration.ShouldBe(TimeSpan.FromHours(8));

        var stages = sleep.Score!.StageSummary!;
        stages.SleepCycleCount.ShouldBe(3);
        stages.DisturbanceCount.ShouldBe(12);
        stages.TotalInBedTime.ShouldBe(TimeSpan.FromMilliseconds(30272735));
        stages.TotalAsleepTime.ShouldBe(TimeSpan.FromMilliseconds(30272735 - 1403507));

        sleep.Score.SleepNeeded!.Total.ShouldBe(
            TimeSpan.FromMilliseconds(27395716 + 352230 + 208595 - 12312));
        sleep.Score.RespiratoryRate.ShouldBe(16.11328125f);
    }

    [Fact]
    public void Reads_a_recovery_including_optional_sensor_values()
    {
        var recovery = Deserialize<Recovery>(SampleJson.Recovery);

        recovery.CycleId.ShouldBe(93845);
        recovery.SleepId.ShouldBe(Guid.Parse("ecfc6a15-4661-442f-a9a4-f160dd7afae8"));
        recovery.Score!.UserCalibrating.ShouldBeFalse();
        recovery.Score.RecoveryScorePercentage.ShouldBe(44f);
        recovery.Score.HrvRmssdMilli.ShouldBe(31.813562f);
        recovery.Score.Spo2Percentage.ShouldBe(95.6875f);
        recovery.Score.SkinTempCelsius.ShouldBe(33.7f);
    }

    [Fact]
    public void Reads_a_workout_with_zone_durations()
    {
        var workout = Deserialize<Workout>(SampleJson.Workout);

        workout.SportName.ShouldBe("running");
        workout.SportId.ShouldBe(1);
        workout.Score!.DistanceMeter.ShouldBe(1772.77035916f);
        workout.Score.AltitudeChangeMeter.ShouldBe(-0.5893480777740479f);
        workout.Score.ZoneDurations!.Total.ShouldBe(
            TimeSpan.FromMilliseconds(13458 + 389951 + 388093 + 620779 + 220020 + 0));
    }

    [Fact]
    public void Reads_the_user_endpoints()
    {
        Deserialize<UserBasicProfile>(SampleJson.BasicProfile).Email.ShouldBe("jsmith123@whoop.com");
        Deserialize<UserBodyMeasurement>(SampleJson.BodyMeasurement).MaxHeartRate.ShouldBe(200);
    }

    [Fact]
    public void Reads_a_partner_service_request()
    {
        var serviceRequest = Deserialize<ServiceRequest>(SampleJson.ServiceRequest);

        serviceRequest.Code.ShouldBe("LIPID-PANEL");
        serviceRequest.TaskBusinessStatus.ShouldBe("SAMPLE_COLLECTED");
    }

    [Fact]
    public void Ignores_properties_the_library_does_not_know_about()
    {
        const string json = """{"user_id":1,"email":"a@b.c","first_name":"A","last_name":"B","new_field":true}""";

        Deserialize<UserBasicProfile>(json).UserId.ShouldBe(1);
    }

    [Fact]
    public void Omits_null_members_when_writing()
    {
        var request = new ServiceRequestStatusRequest { TaskBusinessStatus = "DONE" };

        JsonSerializer.Serialize(request, WhoopJson.Options)
            .ShouldBe("""{"task_business_status":"DONE"}""");
    }

    [Fact]
    public void Round_trips_a_cycle_without_losing_information()
    {
        var original = Deserialize<Cycle>(SampleJson.Cycle);

        var roundTripped = Deserialize<Cycle>(JsonSerializer.Serialize(original, WhoopJson.Options));

        roundTripped.ShouldBe(original);
    }

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, WhoopJson.Options)!;
}
