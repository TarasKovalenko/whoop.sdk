using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Models
{
    /// <summary>A single page of a WHOOP collection endpoint.</summary>
    /// <typeparam name="T">The record type contained in the page.</typeparam>
    public sealed record PaginatedResponse<T>
    {
        /// <summary>The records in this page, in the order returned by the API.</summary>
        [JsonPropertyName("records")]
        public IReadOnlyList<T> Records { get; init; } = new List<T>();

        /// <summary>
        /// Opaque cursor for the next page, or <see langword="null"/> when this is the last page.
        /// </summary>
        [JsonPropertyName("next_token")]
        public string? NextToken { get; init; }
    }
}
