namespace Raider.Media.Internal
{
	internal class MediaPlayInfo : IMediaPlayInfo
	{
		public float Position { get; set; }
		public long Time { get; set; }
		public long Length { get; set; }
		public int Volume { get; set; }
		public bool IsPlaying { get; set; }
	}
}
