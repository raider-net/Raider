using LibVLCSharp.Shared;
using System.Threading.Tasks;

namespace Raider.Media.Audio
{
	public interface IAudioPlayer
	{
		MediaPlayer MediaPlayer { get; }
		LibVLCSharp.Shared.Media? Media { get; }
		bool IsPlaying { get; }
		int TargetVolume { get; }

		void SetTargetVolume(int targetVolume);
		Task SetVolumeAsync(int toVolume, bool progressive);
		Task<bool> PlayAsync(IMediaInfo mediaInfo, int? volume, bool progressiveVolume);
		Task<double?> PauseAsync(bool progressiveVolume);
		Task<bool> ResumeAsync(int? volume, bool progressiveVolume);
		Task<double?> StopAsync(bool removeMedia, bool progressiveVolume);
	}
}
