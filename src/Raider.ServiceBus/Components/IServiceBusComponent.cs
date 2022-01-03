using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Components
{
	public interface IServiceBusComponent
	{
		/// <summary>
		/// On component initialization.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> OnInit(CancellationToken cancellationToken = default);

		/// <summary>
		/// On component started.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> OnStarted(CancellationToken cancellationToken = default);

		/// <summary>
		/// On component stopping.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> OnStopping(CancellationToken cancellationToken = default);

		/// <summary>
		/// On component error.
		/// </summary>
		/// <param name="failureReason">Error reason</param>
		/// <param name="exception">Exception</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult<bool>> OnError(FailureReason failureReason, Exception? exception, CancellationToken cancellationToken = default);
	}
}
