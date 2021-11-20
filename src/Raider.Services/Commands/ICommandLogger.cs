namespace Raider.Services.Commands
{
	public interface ICommandLogger
	{
		void WriteCommandEntry(ICommandEntry entry);
		void WriteCommandExit(ICommandEntry entry, decimal elapsedMilliseconds, bool isError, string? data);
	}
}
