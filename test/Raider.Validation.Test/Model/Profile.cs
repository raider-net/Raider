using System;

namespace Raider.Validation.Test.Model
{
	public class Profile
	{
		public int ProfIntNotNull { get; set; }
		public int? ProfIntNullable { get; set; }
		public decimal ProfDecimalNotNull { get; set; }
		public decimal? ProfDecimalNullable { get; set; }
		public bool ProfBoolNotNull { get; set; }
		public bool? ProfBoolNullable { get; set; }
		public DateTime ProfDateTimeNotNull { get; set; }
		public DateTime? ProfDateTimeNullable { get; set; }
		public Guid ProfGuidNotNull { get; set; }
		public Guid? ProfGuidNullable { get; set; }
		public MyTestEnum ProfEnumNotNull { get; set; }
		public MyTestEnum? ProfEnumNullable { get; set; }
		public string ProfStringNotNull { get; set; }
		public string? ProfStringNullable { get; set; }
	}
}
