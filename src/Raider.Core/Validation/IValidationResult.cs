using System.Collections.Generic;

namespace Raider.Validation
{
	public interface IValidationResult
	{
		IReadOnlyList<IBaseValidationFailure> Errors { get; }
		bool Interrupted { get; }
	}
}
