using LibVLCSharp.Shared;
using System;

namespace Raider.Media.Internal
{
	internal class MediaPlayedInfo : IMediaPlayedInfo
	{
		public IMediaFile MediaFile { get; }
		public DateTime? MediaStartTime { get; set; }
		public DateTime? MediaEndTime { get; set; }
		public double? PlayTimeInSeconds { get; set; }
		public string? MediaError { get; set; }
		public bool? MediaManuallyStopped { get; set; }
		public string? Mrl { get; set; }
		public VLCState? State { get; set; }
		public MediaType? Type { get; set; }

		public MediaPlayedInfo(IMediaFile mediaFile)
		{
			MediaFile = mediaFile;
		}
	}
}
