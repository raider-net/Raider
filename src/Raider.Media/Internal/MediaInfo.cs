using LibVLCSharp.Shared;
using System;

namespace Raider.Media.Internal
{
	internal class MediaInfo : IMediaInfo
	{
		public IMediaFile? MediaFile { get; set; }
		public float Position { get; set; }
		public DateTime? MediaStartTime { get; set; }
		public DateTime? MediaEndTime { get; set; }
		public double? PlayTimeInSeconds { get; set; }
		public long Time { get; set; }
		public long Length { get; set; }
		public int Volume { get; set; }
		public bool IsPlaying { get; set; }
		public string? Mrl { get; set; }
		public VLCState? State { get; set; }
		public MediaType? Type { get; set; }
		public string? MediaError { get; set; }
		public bool? MediaManuallyStopped { get; set; }

		public MediaInfo(IMediaFile mediaFile)
		{
			MediaFile = mediaFile;
		}
	}
}
