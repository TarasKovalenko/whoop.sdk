using System;

namespace Whoop.Sdk.Models
{
    /// <summary>Filtering and paging options shared by every WHOOP collection endpoint.</summary>
    public sealed record WhoopCollectionRequest
    {
        /// <summary>The largest page size the API accepts.</summary>
        public const int MaxLimit = 25;

        private readonly int? _limit;

        /// <summary>
        /// Number of records per page, between 1 and <see cref="MaxLimit"/>. When <see langword="null"/>
        /// the API default (10) is used.
        /// </summary>
        public int? Limit
        {
            get => _limit;
            init
            {
                if (value is < 1 or > MaxLimit)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Limit must be between 1 and {MaxLimit}.");
                }

                _limit = value;
            }
        }

        /// <summary>Return records created on or after this instant.</summary>
        public DateTimeOffset? Start { get; init; }

        /// <summary>Return records created before this instant. Defaults server-side to "now".</summary>
        public DateTimeOffset? End { get; init; }

        /// <summary>
        /// Cursor from a previous page's <see cref="PaginatedResponse{T}.NextToken"/>. Ignored by the
        /// streaming <c>Enumerate</c> helpers, which manage the cursor themselves.
        /// </summary>
        public string? NextToken { get; init; }
    }
}
