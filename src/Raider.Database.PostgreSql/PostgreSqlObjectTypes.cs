namespace Raider.Database.PostgreSql
{
	public enum PostgreSqlObjectTypes
	{
		/// <summary>
		/// ordinary table
		/// </summary>
		r,

		/// <summary>
		/// index
		/// </summary>
		i,

		/// <summary>
		/// sequence
		/// </summary>
		S,

		/// <summary>
		/// TOAST table
		/// </summary>
		t,

		/// <summary>
		/// view
		/// </summary>
		v,

		/// <summary>
		/// materialized view
		/// </summary>
		m,

		/// <summary>
		/// composite type
		/// </summary>
		c,

		/// <summary>
		/// foreign table
		/// </summary>
		f,

		/// <summary>
		/// partitioned table
		/// </summary>
		p
	}
}
