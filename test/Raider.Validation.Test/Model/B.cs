using System;
using System.Collections.Generic;

namespace Raider.Validation.Test.Model
{
	public class B
	{
		public int BIntNotNull { get; set; }
		public int? BIntNullable { get; set; }
		public decimal BDecimalNotNull { get; set; }
		public decimal? BDecimalNullable { get; set; }
		public bool BBoolNotNull { get; set; }
		public bool? BBoolNullable { get; set; }
		public DateTime BDateTimeNotNull { get; set; }
		public DateTime? BDateTimeNullable { get; set; }
		public Guid BGuidNotNull { get; set; }
		public Guid? BGuidNullable { get; set; }
		public MyTestEnum BEnumNotNull { get; set; }
		public MyTestEnum? BEnumNullable { get; set; }
		public string BStringNotNull { get; set; }
		public string? BStringNullable { get; set; }
		public C CNotNull { get; set; }
		public C? CNullable { get; set; }
		public List<BItem> BItemsNotNull { get; set; }
		public List<BItem>? BItemsNullable { get; set; }
	}
}
