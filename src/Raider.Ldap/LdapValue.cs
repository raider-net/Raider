namespace Raider
{
	public class LdapValue
	{
		public string? StringValue { get; set; }
		public byte[]? ByteArrayValue { get; set; }
		public string? Error { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(StringValue)
				? $"byteArray[{ByteArrayValue?.Length}]"
				: StringValue;
		}
	}
}
