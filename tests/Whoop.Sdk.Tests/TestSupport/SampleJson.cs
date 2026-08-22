namespace Whoop.Sdk.Tests.TestSupport;

/// <summary>Response payloads shaped exactly like the ones in the published WHOOP OpenAPI document.</summary>
public static class SampleJson
{
    public const string Cycle = """
        {
          "id": 93845,
          "user_id": 10129,
          "created_at": "2022-04-24T11:25:44.774Z",
          "updated_at": "2022-04-24T14:25:44.774Z",
          "start": "2022-04-24T02:25:44.774Z",
          "end": "2022-04-24T10:25:44.774Z",
          "timezone_offset": "-05:00",
          "score_state": "SCORED",
          "score": {
            "strain": 5.2951527,
            "kilojoule": 8288.297,
            "average_heart_rate": 68,
            "max_heart_rate": 141
          }
        }
        """;

    public const string CyclePage = """
        {
          "records": [
            {
              "id": 93845,
              "user_id": 10129,
              "created_at": "2022-04-24T11:25:44.774Z",
              "updated_at": "2022-04-24T14:25:44.774Z",
              "start": "2022-04-24T02:25:44.774Z",
              "end": null,
              "timezone_offset": "-05:00",
              "score_state": "PENDING_SCORE"
            }
          ],
          "next_token": "MTIzOjEyMzEyMwo="
        }
        """;

    public const string Sleep = """
        {
          "id": "ecfc6a15-4661-442f-a9a4-f160dd7afae8",
          "cycle_id": 93845,
          "v1_id": 10235,
          "user_id": 10129,
          "created_at": "2022-04-24T11:25:44.774Z",
          "updated_at": "2022-04-24T14:25:44.774Z",
          "start": "2022-04-24T02:25:44.774Z",
          "end": "2022-04-24T10:25:44.774Z",
          "timezone_offset": "-05:00",
          "nap": false,
          "score_state": "SCORED",
          "score": {
            "stage_summary": {
              "total_in_bed_time_milli": 30272735,
              "total_awake_time_milli": 1403507,
              "total_no_data_time_milli": 0,
              "total_light_sleep_time_milli": 14905851,
              "total_slow_wave_sleep_time_milli": 6630370,
              "total_rem_sleep_time_milli": 5879573,
              "sleep_cycle_count": 3,
              "disturbance_count": 12
            },
            "sleep_needed": {
              "baseline_milli": 27395716,
              "need_from_sleep_debt_milli": 352230,
              "need_from_recent_strain_milli": 208595,
              "need_from_recent_nap_milli": -12312
            },
            "respiratory_rate": 16.11328125,
            "sleep_performance_percentage": 98.0,
            "sleep_consistency_percentage": 90.0,
            "sleep_efficiency_percentage": 91.69533
          }
        }
        """;

    public const string Recovery = """
        {
          "cycle_id": 93845,
          "sleep_id": "ecfc6a15-4661-442f-a9a4-f160dd7afae8",
          "user_id": 10129,
          "created_at": "2022-04-24T11:25:44.774Z",
          "updated_at": "2022-04-24T14:25:44.774Z",
          "score_state": "SCORED",
          "score": {
            "user_calibrating": false,
            "recovery_score": 44.0,
            "resting_heart_rate": 64.0,
            "hrv_rmssd_milli": 31.813562,
            "spo2_percentage": 95.6875,
            "skin_temp_celsius": 33.7
          }
        }
        """;

    public const string Workout = """
        {
          "id": "ecfc6a15-4661-442f-a9a4-f160dd7afae8",
          "v1_id": 1043,
          "user_id": 9012,
          "created_at": "2022-04-24T11:25:44.774Z",
          "updated_at": "2022-04-24T14:25:44.774Z",
          "start": "2022-04-24T02:25:44.774Z",
          "end": "2022-04-24T10:25:44.774Z",
          "timezone_offset": "-05:00",
          "sport_name": "running",
          "score_state": "SCORED",
          "sport_id": 1,
          "score": {
            "strain": 8.2463,
            "average_heart_rate": 123,
            "max_heart_rate": 146,
            "kilojoule": 1569.34033203125,
            "percent_recorded": 100.0,
            "distance_meter": 1772.77035916,
            "altitude_gain_meter": 46.64384460449,
            "altitude_change_meter": -0.5893480777740479,
            "zone_durations": {
              "zone_zero_milli": 13458,
              "zone_one_milli": 389951,
              "zone_two_milli": 388093,
              "zone_three_milli": 620779,
              "zone_four_milli": 220020,
              "zone_five_milli": 0
            }
          }
        }
        """;

    public const string BasicProfile = """
        {
          "user_id": 10129,
          "email": "jsmith123@whoop.com",
          "first_name": "John",
          "last_name": "Smith"
        }
        """;

    public const string BodyMeasurement = """
        {
          "height_meter": 1.8288,
          "weight_kilogram": 90.7185,
          "max_heart_rate": 200
        }
        """;

    public const string ServiceRequest = """
        {
          "id": "1a2b3c4d-5e6f-4a7b-8c9d-0e1f2a3b4c5d",
          "status": "active",
          "intent": "order",
          "code": "LIPID-PANEL",
          "task_business_status": "SAMPLE_COLLECTED",
          "task_description": "Collect blood sample"
        }
        """;

    public const string PartnerToken = """
        {
          "access_token": "partner-token",
          "expires_in": 3600,
          "token_type": "Bearer"
        }
        """;
}
