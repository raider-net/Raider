namespace Raider.Services
{
	public enum OnBeforeAspectContinuation
	{
		CallParentAspect = 1,
		SkipParentAspects = 2,
		StopExecution = 3
	}
}
