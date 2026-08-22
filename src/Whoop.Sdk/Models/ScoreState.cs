namespace Whoop.Sdk.Models
{
    /// <summary>Whether the WHOOP scoring pipeline has produced a score for a record.</summary>
    public enum ScoreState
    {
        /// <summary>The API returned a value this library does not recognise yet.</summary>
        Unknown = 0,

        /// <summary>Scoring completed; the record's <c>Score</c> property is populated.</summary>
        Scored = 1,

        /// <summary>Scoring has not completed yet; retry later.</summary>
        PendingScore = 2,

        /// <summary>The record can never be scored, typically because too little data was captured.</summary>
        Unscorable = 3,
    }
}
