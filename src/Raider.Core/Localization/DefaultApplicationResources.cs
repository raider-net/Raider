namespace Raider.Localization
{
	public class DefaultApplicationResources : IApplicationResources
	{
		public string GlobalExceptionMessage => "An unexpected error occurred.";
		public string DataNotFoundException => "The requested data was not found.";
		public string DataForbiddenException => "You do not have sufficient privileges to perform the operation.";
		public string OptimisticConcurrencyException => "Operation failed because another user has updated or deleted the record. Your changes have been lost. Please review their changes before trying again.";
	}
}
