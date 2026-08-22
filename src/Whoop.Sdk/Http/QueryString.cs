using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Whoop.Sdk.Http
{
    /// <summary>Builds query strings with stable ordering and correct escaping.</summary>
    internal static class QueryString
    {
        /// <summary>The format WHOOP expects for date-time query parameters.</summary>
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>Renders parameters as <c>?a=1&amp;b=2</c>, skipping null values. Returns an empty string when nothing remains.</summary>
        public static string Build(IEnumerable<KeyValuePair<string, string?>>? parameters)
        {
            if (parameters is null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                if (parameter.Value is null)
                {
                    continue;
                }

                builder.Append(builder.Length == 0 ? '?' : '&');
                builder.Append(Uri.EscapeDataString(parameter.Key));
                builder.Append('=');
                builder.Append(Uri.EscapeDataString(parameter.Value));
            }

            return builder.ToString();
        }

        /// <summary>Formats an instant as the UTC, millisecond-precision string the API expects.</summary>
        public static string? Format(DateTimeOffset? value) =>
            value?.ToUniversalTime().ToString(DateTimeFormat, CultureInfo.InvariantCulture);

        /// <summary>Formats an integer using the invariant culture.</summary>
        public static string? Format(int? value) =>
            value?.ToString(CultureInfo.InvariantCulture);
    }
}
