using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raider.Media.Audio
{
	public class AudioMultiPlayer
	{
		public enum MultiPlayMode
		{
			PauseAllButThis = 0,
			StopAllButThis = 1
		}

		public readonly LibVLC _libVLC;
		public readonly List<IAudioPlayer> _audioMediaPlayer;

		public event Action<int, IMediaPlayInfo>? OnStop;
		public event Action<string>? OnError;

		public AudioMultiPlayer(int mediaPlayersCount)
		{
			if (mediaPlayersCount < 1)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayersCount), $"{nameof(mediaPlayersCount)} < 1");

			Core.Initialize();
			_libVLC = new LibVLC();
			_libVLC.Log += (object? sender, LogEventArgs e) =>
			{
				if (e.Level == LogLevel.Error)
				{
					var error = e?.FormattedLog;
					if (!string.IsNullOrWhiteSpace(error))
						OnError?.Invoke(error);
				}
			};

			_audioMediaPlayer = new List<IAudioPlayer>(mediaPlayersCount);
			for (int i = 0; i < mediaPlayersCount; i++)
			{
				var index = i;
				var player = new AudioPlayer(_libVLC, 100);
				player.OnStop += (IMediaPlayInfo mediaPlayInfo) => OnStop?.Invoke(index, mediaPlayInfo);
				_audioMediaPlayer.Add(player);
			}
		}

		public async Task SetVolumeAsync(int mediaPlayerIndex, int volume, bool progressiveVolume = false)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayer.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayer)}.Count <= {mediaPlayerIndex}");

			await _audioMediaPlayer[mediaPlayerIndex].SetVolumeAsync(volume, progressiveVolume);
		}

		public void SetTargetVolume(int mediaPlayerIndex, int volume)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayer.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayer)}.Count <= {mediaPlayerIndex}");

			_audioMediaPlayer[mediaPlayerIndex].SetTargetVolume(volume);
		}

		public async Task<bool> PlayAsync(int mediaPlayerIndex, IMediaInfo mediaInfo, int? volume = null, bool progressiveVolume = false, MultiPlayMode? multiPlayMode = MultiPlayMode.PauseAllButThis)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayer.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayer)}.Count <= {mediaPlayerIndex}");

			await ApplyMultiPlayMode(mediaPlayerIndex, multiPlayMode, progressiveVolume);
			return await _audioMediaPlayer[mediaPlayerIndex].PlayAsync(mediaInfo, volume, progressiveVolume);
		}

		public Task<double?> PauseAsync(int mediaPlayerIndex, bool progressiveVolume = false)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayer.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayer)}.Count <= {mediaPlayerIndex}");

			return _audioMediaPlayer[mediaPlayerIndex].PauseAsync(progressiveVolume);
		}

		public async Task<bool> ResumeAsync(int mediaPlayerIndex, int? volume, bool progressiveVolume = false, MultiPlayMode? multiPlayMode = MultiPlayMode.PauseAllButThis)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayer.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayer)}.Count <= {mediaPlayerIndex}");

			await ApplyMultiPlayMode(mediaPlayerIndex, multiPlayMode, progressiveVolume);
			return await _audioMediaPlayer[mediaPlayerIndex].ResumeAsync(volume, progressiveVolume);
		}

		public Task<double?> StopAsync(int mediaPlayerIndex, bool removeMedia = true, bool progressiveVolume = false)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayer.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayer)}.Count <= {mediaPlayerIndex}");

			return _audioMediaPlayer[mediaPlayerIndex].StopAsync(removeMedia, progressiveVolume);
		}

		private async Task ApplyMultiPlayMode(int currentMediaPlayerIndex, MultiPlayMode? multiPlayMode, bool progressiveVolume)
		{
			if (multiPlayMode == MultiPlayMode.PauseAllButThis)
			{
				for (int i = 0; i < _audioMediaPlayer.Count; i++)
					if (i != currentMediaPlayerIndex)
						await _audioMediaPlayer[i].PauseAsync(progressiveVolume);
			}
			else if (multiPlayMode == MultiPlayMode.StopAllButThis)
			{
				for (int i = 0; i < _audioMediaPlayer.Count; i++)
					if (i != currentMediaPlayerIndex)
						await _audioMediaPlayer[i].StopAsync(false, progressiveVolume);
			}
		}
	}
}
