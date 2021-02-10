using System;

namespace Raider.Validation.Test.Model
{
	public class A
	{
		public int AIntNotNull { get; set; }
		public int? AIntNullable { get; set; }
		public decimal ADecimalNotNull { get; set; }
		public decimal? ADecimalNullable { get; set; }
		public bool ABoolNotNull { get; set; }
		public bool? ABoolNullable { get; set; }
		public DateTime ADateTimeNotNull { get; set; }
		public DateTime? ADateTimeNullable { get; set; }
		public Guid AGuidNotNull { get; set; }
		public Guid? AGuidNullable { get; set; }
		public MyTestEnum AEnumNotNull { get; set; }
		public MyTestEnum? AEnumNullable { get; set; }
		public string AStringNotNull { get; set; }
		public string? AStringNullable { get; set; }
		public B BNotNull { get; set; }
		public B? BNullable { get; set; }
	}
}
