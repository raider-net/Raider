namespace Raider.QueryServices.Queries
{
	public interface IQueryLogger
	{
		void WriteQueryEntry(IQueryEntry entry);
		void WriteQueryExit(IQueryEntry entry, decimal elapsedMilliseconds);
	}
}
