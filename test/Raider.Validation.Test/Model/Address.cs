using System;

namespace Raider.Validation.Test.Model
{
	public class Address
	{
		public int AddIntNotNull { get; set; }
		public int? AddIntNullable { get; set; }
		public decimal AddDecimalNotNull { get; set; }
		public decimal? AddDecimalNullable { get; set; }
		public bool AddBoolNotNull { get; set; }
		public bool? AddBoolNullable { get; set; }
		public DateTime AddDateTimeNotNull { get; set; }
		public DateTime? AddDateTimeNullable { get; set; }
		public Guid AddGuidNotNull { get; set; }
		public Guid? AddGuidNullable { get; set; }
		public MyTestEnum AddEnumNotNull { get; set; }
		public MyTestEnum? AddEnumNullable { get; set; }
		public string AddStringNotNull { get; set; }
		public string? AddStringNullable { get; set; }
		public Profile AddProfileNotNull { get; set; }
		public Profile? AddProfileNullable { get; set; }
	}
}
