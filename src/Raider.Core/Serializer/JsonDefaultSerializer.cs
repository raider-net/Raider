#if NET5_0
namespace Raider.Serializer
{
	public static class JsonDefaultSerializerOptions
	{
		public static readonly System.Text.Json.JsonSerializerOptions JsonSerializerOptions =
			new System.Text.Json.JsonSerializerOptions
			{
				WriteIndented = false,
				ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never //System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
			};
	}
}
#endif
