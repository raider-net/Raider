using System;

namespace Raider.Media
{
	public interface IMediaPlayInfo
	{
		IMediaInfo MediaInfo { get; }
		DateTime? MediaStartTime { get; }
		DateTime? MediaEndTime { get; }
		double? PlayTimeInSeconds { get; }
		string? MediaError { get; }
		bool? MediaManuallyStopped { get; }
	}
}
