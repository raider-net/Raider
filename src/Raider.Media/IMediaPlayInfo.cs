using LibVLCSharp.Shared;

namespace Raider.Media
{
	public interface IMediaPlayInfo
	{
		/// <summary>
		/// true if the media player is playing, false otherwise
		/// </summary>
		bool IsPlaying { get; }

		/// <summary>
		/// Get movie position as percentage between 0.0 and 1.0. or -1 if there is no media.
		/// </summary>
		float Position { get; }

		/// <summary>
		/// Get the movie time (in ms), or -1 if there is no media.
		/// </summary>
		long Time { get; }

		/// <summary>
		/// The movie length (in ms), or -1 if there is no media.
		/// </summary>
		long Length { get; }

		/// <summary>
		/// Get/Set the volume in percents (0 = mute, 100 = 0dB)
		/// </summary>
		int Volume { get; }

		/// <summary>
		/// Get the media resource locator (mrl) from a media descriptor object
		/// </summary>
		string? Mrl { get; }

		/// <summary>
		/// Get current LibVLCSharp.Shared.VLCState of media descriptor object.
		/// </summary>
		VLCState? State { get; }

		/// <summary>
		/// The type of the media
		/// </summary>
		MediaType? Type { get; }
	}
}
