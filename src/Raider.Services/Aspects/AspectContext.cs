using Raider.Commands;
using Raider.Logging;
using Raider.Services.Commands;
using System;
using System.Threading;

namespace Raider.Services
{
	public class AspectContext
	{
		public ServiceContext ServiceContext { get; }
		public MethodLogScope MethodScope { get; }
		public CancellationToken CancellationToken { get; }

		internal AspectContext(ServiceContext serviceContext, MethodLogScope methodScope, CancellationToken cancellationToken = default)
		{
			ServiceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
			MethodScope = methodScope ?? throw new ArgumentNullException(nameof(methodScope));
			CancellationToken = cancellationToken;
		}
	}

	public class WorkflowActionContext<TResult>
	{
		public ServiceContext ServiceContext { get; }
		public MethodLogScope MethodScope { get; }
		public CancellationToken CancellationToken { get; }
		public CommandResultBuilder<TResult> CommandResultBuilder { get; }
		public ICommandResult<TResult> Result { get; }

		internal WorkflowActionContext(ServiceContext serviceContext, MethodLogScope methodScope, CancellationToken cancellationToken = default)
		{
			ServiceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
			MethodScope = methodScope ?? throw new ArgumentNullException(nameof(methodScope));
			CancellationToken = cancellationToken;
			CommandResultBuilder = new CommandResultBuilder<TResult>();
			Result = CommandResultBuilder.Build();
		}
	}
}
