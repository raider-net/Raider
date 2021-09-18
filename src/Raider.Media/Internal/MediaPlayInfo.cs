using LibVLCSharp.Shared;

namespace Raider.Media.Internal
{
	internal class MediaPlayInfo : IMediaPlayInfo
	{
		public IMediaFile? MediaFile { get; set; }
		public float Position { get; set; }
		public long Time { get; set; }
		public long Length { get; set; }
		public int Volume { get; set; }
		public bool IsPlaying { get; set; }
		public string? Mrl { get; set; }
		public VLCState? State { get; set; }
		public MediaType? Type { get; set; }
	}
}
