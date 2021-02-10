using System;

namespace Raider.Validation.Test.Model
{
	public class E
	{
		public int EIntNotNull { get; set; }
		public int? EIntNullable { get; set; }
		public decimal EDecimalNotNull { get; set; }
		public decimal? EDecimalNullable { get; set; }
		public bool EBoolNotNull { get; set; }
		public bool? EBoolNullable { get; set; }
		public DateTime EDateTimeNotNull { get; set; }
		public DateTime? EDateTimeNullable { get; set; }
		public Guid EGuidNotNull { get; set; }
		public Guid? EGuidNullable { get; set; }
		public MyTestEnum EEnumNotNull { get; set; }
		public MyTestEnum? EEnumNullable { get; set; }
		public string EStringNotNull { get; set; }
		public string? EStringNullable { get; set; }
		public F FNotNull { get; set; }
		public F? FNullable { get; set; }
	}
}
