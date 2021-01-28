namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database view.
	/// </summary>
	public class DatabaseView : DatabaseColumnObject
	{
		public string? ViewDefinition { get; set; }

		public DatabaseView()
		{

		}

		public DatabaseView(DatabaseObject obj)
		{
			Schema = obj.Schema;
			ObjectId = obj.ObjectId;
			Name = obj.Name;
			ObjectType = obj.ObjectType;
		}

		public DatabaseView BuildView()
		{
			BuildDatabaseObject();

			Schema.Views.Add(this);

			return this;
		}
	}
}
