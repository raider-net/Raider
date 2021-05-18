using System.IO;

namespace Raider.Web
{
	public class FormFile
	{
		public Stream? Content { get; set; }
		public byte[]? Data { get; set; }
		public string? FileName { get; set; }
		public string? ContentType { get; set; }
		public long? Length { get; set; }

		/// <summary>
		/// Form field name
		/// </summary>
		public string? Name { get; set; }

		public Stream? OpenReadStream()
		{
			Stream? stream = null;
			if (Content != null)
				stream = Content;
			else if (Data != null)
				stream = new MemoryStream(Data);

			if (stream?.CanSeek == true)
				stream.Seek(0, SeekOrigin.Begin);

			return stream;
		}
	}
}
