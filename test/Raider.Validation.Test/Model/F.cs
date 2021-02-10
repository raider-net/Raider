using System;

namespace Raider.Validation.Test.Model
{
	public class F
	{
		public int FIntNotNull { get; set; }
		public int? FIntNullable { get; set; }
		public decimal FDecimalNotNull { get; set; }
		public decimal? FDecimalNullable { get; set; }
		public bool FBoolNotNull { get; set; }
		public bool? FBoolNullable { get; set; }
		public DateTime FDateTimeNotNull { get; set; }
		public DateTime? FDateTimeNullable { get; set; }
		public Guid FGuidNotNull { get; set; }
		public Guid? FGuidNullable { get; set; }
		public MyTestEnum FEnumNotNull { get; set; }
		public MyTestEnum? FEnumNullable { get; set; }
		public string FStringNotNull { get; set; }
		public string? FStringNullable { get; set; }
	}
}
