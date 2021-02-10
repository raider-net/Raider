using System;

namespace Raider.Validation.Test.Model
{
	public class D
	{
		public int DIntNotNull { get; set; }
		public int? DIntNullable { get; set; }
		public decimal DDecimalNotNull { get; set; }
		public decimal? DDecimalNullable { get; set; }
		public bool DBoolNotNull { get; set; }
		public bool? DBoolNullable { get; set; }
		public DateTime DDateTimeNotNull { get; set; }
		public DateTime? DDateTimeNullable { get; set; }
		public Guid DGuidNotNull { get; set; }
		public Guid? DGuidNullable { get; set; }
		public MyTestEnum DEnumNotNull { get; set; }
		public MyTestEnum? DEnumNullable { get; set; }
		public string DStringNotNull { get; set; }
		public string? DStringNullable { get; set; }
		public E ENotNull { get; set; }
		public E? ENullable { get; set; }
	}
}
