using System;

namespace Raider.Media
{
	internal class MediaPlayInfo : IMediaPlayInfo
	{
		public IMediaInfo MediaInfo { get; }
		public DateTime? MediaStartTime { get; set; }
		public DateTime? MediaEndTime { get; set; }
		public double? PlayTimeInSeconds { get; set; }
		public string? MediaError { get; set; }
		public bool? MediaManuallyStopped { get; set; }

		public MediaPlayInfo(IMediaInfo mediaInfo)
		{
			MediaInfo = mediaInfo;
		}
	}
}
