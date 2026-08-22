using System.Threading;
using System.Threading.Tasks;

namespace Whoop.Sdk.Authentication
{
    /// <summary>Supplies the bearer token attached to outgoing WHOOP requests.</summary>
    public interface IWhoopTokenProvider
    {
        /// <summary>Returns a currently valid access token, acquiring or refreshing one if necessary.</summary>
        /// <param name="cancellationToken">Cancels the operation.</param>
        /// <returns>The access token, without the <c>Bearer</c> prefix.</returns>
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
