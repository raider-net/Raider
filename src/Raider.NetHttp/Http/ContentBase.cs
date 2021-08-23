using Raider.NetHttp.Http.Headers;

namespace Raider.NetHttp.Http
{
	public abstract class ContentBase
	{
		public ContentHeaders Headers { get; }
		public bool ClearDefaultHeaders { get; set; }

		public ContentBase()
		{
			Headers = new ContentHeaders();
		}
	}
}
