namespace Raider.Sql.Metadata
{
	public enum DatabaseObjectTypes
	{
		UNDEFINED,
		CheckConstraint,
		DefaultValue,
		ForeignKey,

		/// <summary>
		/// Table valued function
		/// </summary>
		Function,
		Index,
		MaterializedView,
		PrimaryKey,
		Sequence,
		ScalarFunction,
		StoredProcedure,

		/// <summary>
		/// User defined table
		/// </summary>
		Table,
		Trigger,
		UniqueConstraint,
		View
	}
}
