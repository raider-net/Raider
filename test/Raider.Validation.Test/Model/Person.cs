using System;
using System.Collections.Generic;

namespace Raider.Validation.Test.Model
{
	public class Person
	{
		public int MyIntNotNull { get; set; }
		public int? MyIntNullable { get; set; }
		public decimal MyDecimalNotNull { get; set; }
		public decimal? MyDecimalNullable { get; set; }
		public bool MyBoolNotNull { get; set; }
		public bool? MyBoolNullable { get; set; }
		public DateTime MyDateTimeNotNull { get; set; }
		public DateTime? MyDateTimeNullable { get; set; }
		public Guid MyGuidNotNull { get; set; }
		public Guid? MyGuidNullable { get; set; }
		public MyTestEnum MyEnumNotNull { get; set; }
		public MyTestEnum? MyEnumNullable { get; set; }
		public string MyStringNotNull { get; set; }
		public string? MyStringNullable { get; set; }
		public A ANotNull { get; set; }
		public A? ANullable { get; set; }
		public List<Address> MyAddressesNotNull { get; set; }
		public List<Address>? MyAddressesNullable { get; set; }
	}
}
