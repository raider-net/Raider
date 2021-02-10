using System;
using System.Collections.Generic;

namespace Raider.Validation.Test.Model
{
	public class C
	{
		public int CIntNotNull { get; set; }
		public int? CIntNullable { get; set; }
		public decimal CDecimalNotNull { get; set; }
		public decimal? CDecimalNullable { get; set; }
		public bool CBoolNotNull { get; set; }
		public bool? CBoolNullable { get; set; }
		public DateTime CDateTimeNotNull { get; set; }
		public DateTime? CDateTimeNullable { get; set; }
		public Guid CGuidNotNull { get; set; }
		public Guid? CGuidNullable { get; set; }
		public MyTestEnum CEnumNotNull { get; set; }
		public MyTestEnum? CEnumNullable { get; set; }
		public string CStringNotNull { get; set; }
		public string? CStringNullable { get; set; }
		public D DNotNull { get; set; }
		public D? DNullable { get; set; }
		public List<CItem> CItemsNotNull { get; set; }
		public List<CItem>? CItemsNullable { get; set; }
	}
}
