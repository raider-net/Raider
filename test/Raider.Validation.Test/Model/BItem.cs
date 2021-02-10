using System;

namespace Raider.Validation.Test.Model
{
	public class BItem
	{
		public int BItemIntNotNull { get; set; }
		public int? BItemIntNullable { get; set; }
		public decimal BItemDecimalNotNull { get; set; }
		public decimal? BItemDecimalNullable { get; set; }
		public bool BItemBoolNotNull { get; set; }
		public bool? BItemBoolNullable { get; set; }
		public DateTime BItemDateTimeNotNull { get; set; }
		public DateTime? BItemDateTimeNullable { get; set; }
		public Guid BItemGuidNotNull { get; set; }
		public Guid? BItemGuidNullable { get; set; }
		public MyTestEnum BItemEnumNotNull { get; set; }
		public MyTestEnum? BItemEnumNullable { get; set; }
		public string BItemStringNotNull { get; set; }
		public string? BItemStringNullable { get; set; }
	}
}
