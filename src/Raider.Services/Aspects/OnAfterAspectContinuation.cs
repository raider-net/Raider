namespace Raider.Services
{
	public enum OnAfterAspectContinuation
	{
		MergeMessagesAndCallParentAspect = 1,
		MergeMessagesAndSkipParentAspects = 2,
		MergeMessagesRemoveResultAndCallParentAspect = 3,
		MergeMessagesRemoveResultAndSkipParentAspects = 4
	}
}
