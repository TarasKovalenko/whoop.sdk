// The trusted-partner lab flow: client credentials, read a requisition, push results back.
//
//   export WHOOP_PARTNER_CLIENT_ID="..."
//   export WHOOP_PARTNER_CLIENT_SECRET="..."
//   dotnet run --project samples/Whoop.Sdk.Samples.TrustedPartner -- <requisition-id>
//
// Only accounts WHOOP has onboarded as lab partners can call these endpoints.

using Microsoft.Extensions.DependencyInjection;
using Whoop.Sdk;
using Whoop.Sdk.Extensions.DependencyInjection;
using Whoop.Sdk.Models;

var clientId = Environment.GetEnvironmentVariable("WHOOP_PARTNER_CLIENT_ID");
var clientSecret = Environment.GetEnvironmentVariable("WHOOP_PARTNER_CLIENT_SECRET");

if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
{
    Console.Error.WriteLine("Set WHOOP_PARTNER_CLIENT_ID and WHOOP_PARTNER_CLIENT_SECRET.");
    return 1;
}

if (args.Length == 0 || !Guid.TryParse(args[0], out var requisitionId))
{
    Console.Error.WriteLine("Pass a requisition id as the first argument.");
    return 1;
}

var services = new ServiceCollection();

// Tokens are fetched over a separate, unauthenticated pipeline so acquiring one cannot recurse
// back through the authentication handler.
services.AddWhoopPartnerAuthentication(clientId, clientSecret);
services.AddWhoopClient();

await using var provider = services.BuildServiceProvider();
var whoop = provider.GetRequiredService<IWhoopClient>();

try
{
    var requisition = await whoop.Partner.GetLabRequisitionAsync(requisitionId);

    Console.WriteLine($"Requisition {requisition.Id} for patient {requisition.Patient?.Id}");
    foreach (var appointment in requisition.Appointments)
    {
        Console.WriteLine($"  appointment {appointment.StartTime:g} covers {appointment.ServiceRequestIds.Count} request(s)");
    }

    foreach (var serviceRequest in requisition.ServiceRequests)
    {
        Console.WriteLine($"  {serviceRequest.Code,-16} {serviceRequest.Status,-10} {serviceRequest.TaskBusinessStatus}");
    }

    if (requisition.ServiceRequests.Count == 0)
    {
        Console.WriteLine("Nothing to fulfil.");
        return 0;
    }

    var first = requisition.ServiceRequests[0];

    // Move the task along, then upload the observations.
    var updated = await whoop.Partner.UpdateServiceRequestStatusAsync(
        first.Id,
        new ServiceRequestStatusRequest
        {
            TaskBusinessStatus = "SAMPLE_COLLECTED",
            Reason = "Collected at the draw site",
        });

    Console.WriteLine($"Service request {updated.Id} is now {updated.TaskBusinessStatus}.");

    await whoop.Partner.UploadDiagnosticReportResultsAsync(
        first.Id,
        new DiagnosticReportCreateRequest
        {
            Status = "final",
            Observations = new[]
            {
                new CreateObservationRequest
                {
                    Code = "2093-3",
                    ValueNumeric = 178,
                    Unit = "mg/dL",
                    Status = "final",
                },
                new CreateObservationRequest
                {
                    Code = "2085-9",
                    ValueNumeric = 54,
                    Unit = "mg/dL",
                    Status = "final",
                },
            },
        });

    Console.WriteLine("Results uploaded.");
    return 0;
}
catch (WhoopApiException exception) when (exception.IsAuthenticationFailure)
{
    Console.Error.WriteLine("Partner credentials rejected.");
    return 3;
}
catch (WhoopApiException exception)
{
    Console.Error.WriteLine($"{exception.Message}\n{exception.ResponseBody}");
    return 4;
}
