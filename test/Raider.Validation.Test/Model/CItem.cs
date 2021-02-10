using System;

namespace Raider.Validation.Test.Model
{
	public class CItem
	{
		public int CItemIntNotNull { get; set; }
		public int? CItemIntNullable { get; set; }
		public decimal CItemDecimalNotNull { get; set; }
		public decimal? CItemDecimalNullable { get; set; }
		public bool CItemBoolNotNull { get; set; }
		public bool? CItemBoolNullable { get; set; }
		public DateTime CItemDateTimeNotNull { get; set; }
		public DateTime? CItemDateTimeNullable { get; set; }
		public Guid CItemGuidNotNull { get; set; }
		public Guid? CItemGuidNullable { get; set; }
		public MyTestEnum CItemEnumNotNull { get; set; }
		public MyTestEnum? CItemEnumNullable { get; set; }
		public string CItemStringNotNull { get; set; }
		public string? CItemStringNullable { get; set; }
	}
}
