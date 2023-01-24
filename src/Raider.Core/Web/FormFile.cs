using System;
using System.IO;

namespace Raider.Web
{
	public class FormFile
	{
		public Guid? Id { get; set; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
		[Newtonsoft.Json.JsonIgnore]
#elif NET5_0
		[System.Text.Json.Serialization.JsonIgnore]
#endif
		public Stream? Content { get; set; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
		[Newtonsoft.Json.JsonIgnore]
#elif NET5_0
		[System.Text.Json.Serialization.JsonIgnore]
#endif
		public byte[]? Data { get; set; }

		public string? FileName { get; set; }
		public string? ContentType { get; set; }
		public long? Length { get; set; }
		public string? Tag { get; set; }
		public string? Hash { get; set; }
		public int DbOperation { get; set; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
		[Newtonsoft.Json.JsonIgnore]
#elif NET5_0
		[System.Text.Json.Serialization.JsonIgnore]
#endif
		public bool HasContentData => Content != null || Data != null;

		public Stream? OpenReadStream(bool asMemoryStream = false)
		{
			Stream? stream = null;
			if (Content != null)
			{
				if (Content.CanSeek == true)
					Content.Seek(0, SeekOrigin.Begin);

				if (asMemoryStream)
				{
					var memoryStream = new MemoryStream();
					Content.CopyTo(memoryStream);
					stream = memoryStream;
				}
				else
				{
					stream = Content;
				}
			}
			else if (Data != null)
				stream = new MemoryStream(Data);

			if (stream?.CanSeek == true)
				stream.Seek(0, SeekOrigin.Begin);

			return stream;
		}
	}
}
