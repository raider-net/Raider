using LibVLCSharp.Shared;
using System;

namespace Raider.Media
{
	public interface IMediaPlayedInfo
	{
		IMediaFile MediaFile { get; }
		DateTime? MediaStartTime { get; }
		DateTime? MediaEndTime { get; }
		double? PlayTimeInSeconds { get; }
		string? MediaError { get; }
		bool? MediaManuallyStopped { get; }
		string? Mrl { get; }
		VLCState? State { get; }
		MediaType? Type { get; }
	}
}
