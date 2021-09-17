using LibVLCSharp.Shared;
using Raider.Media.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
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

		private readonly LibVLC _libVLC;
		private readonly List<IAudioPlayer> _audioMediaPlayers;

		public event Action<int, IMediaPlayedInfo>? OnStop;
		public event Action<string>? OnError;
		public event Func<int, IMediaPlayedInfo, Task>? OnStopAsync;
		public event Func<string, Task>? OnErrorAsync;

		public AudioMultiPlayer(int mediaPlayersCount)
		{
			if (mediaPlayersCount < 1)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayersCount), $"{nameof(mediaPlayersCount)} < 1");

			Core.Initialize();
			_libVLC = new LibVLC();
			_libVLC.Log += async (object? sender, LogEventArgs e) =>
			{
				if (e.Level == LogLevel.Error)
				{
					var error = e?.FormattedLog;
					if (!string.IsNullOrWhiteSpace(error))
					{
						OnError?.Invoke(error);
						if (OnErrorAsync != null)
							await OnErrorAsync.Invoke(error);
					}
				}
			};

			_audioMediaPlayers = new List<IAudioPlayer>(mediaPlayersCount);
			for (int i = 0; i < mediaPlayersCount; i++)
			{
				var index = i;
				var player = new AudioPlayer(_libVLC, 100);
				player.OnStop += (IMediaPlayedInfo mediaPlayedInfo) => OnStop?.Invoke(index, mediaPlayedInfo);
				player.OnStopAsync +=
					async (IMediaPlayedInfo mediaPlayedInfo) =>
					{
						if (OnStopAsync != null)
							await OnStopAsync.Invoke(index, mediaPlayedInfo);
					};
				_audioMediaPlayers.Add(player);
			}
		}

		public async Task SetVolumeAsync(int mediaPlayerIndex, int volume, bool progressiveVolume = false)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayers.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayers)}.Count <= {mediaPlayerIndex}");

			await _audioMediaPlayers[mediaPlayerIndex].SetVolumeAsync(volume, progressiveVolume);
		}

		public void SetTargetVolume(int mediaPlayerIndex, int volume)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayers.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayers)}.Count <= {mediaPlayerIndex}");

			_audioMediaPlayers[mediaPlayerIndex].SetTargetVolume(volume);
		}

		public async Task<bool> PlayAsync(int mediaPlayerIndex, IMediaFile mediaFile, int? volume = null, bool progressiveVolume = false, MultiPlayMode? multiPlayMode = MultiPlayMode.PauseAllButThis)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayers.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayers)}.Count <= {mediaPlayerIndex}");

			await ApplyMultiPlayMode(mediaPlayerIndex, multiPlayMode, progressiveVolume);
			return await _audioMediaPlayers[mediaPlayerIndex].PlayAsync(mediaFile, volume, progressiveVolume);
		}

		public Task<double?> PauseAsync(int mediaPlayerIndex, bool progressiveVolume = false)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayers.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayers)}.Count <= {mediaPlayerIndex}");

			return _audioMediaPlayers[mediaPlayerIndex].PauseAsync(progressiveVolume);
		}

		public async Task<bool> ResumeAsync(int mediaPlayerIndex, int? volume, bool progressiveVolume = false, MultiPlayMode? multiPlayMode = MultiPlayMode.PauseAllButThis)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayers.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayers)}.Count <= {mediaPlayerIndex}");

			await ApplyMultiPlayMode(mediaPlayerIndex, multiPlayMode, progressiveVolume);
			return await _audioMediaPlayers[mediaPlayerIndex].ResumeAsync(volume, progressiveVolume);
		}

		public Task<double?> StopAsync(int mediaPlayerIndex, bool removeMedia = true, bool progressiveVolume = false)
		{
			if (mediaPlayerIndex < 0 || _audioMediaPlayers.Count <= mediaPlayerIndex)
				throw new ArgumentOutOfRangeException(nameof(mediaPlayerIndex), $"{nameof(mediaPlayerIndex)} < 0 || {nameof(_audioMediaPlayers)}.Count <= {mediaPlayerIndex}");

			return _audioMediaPlayers[mediaPlayerIndex].StopAsync(removeMedia, progressiveVolume);
		}

		private async Task ApplyMultiPlayMode(int currentMediaPlayerIndex, MultiPlayMode? multiPlayMode, bool progressiveVolume)
		{
			if (multiPlayMode == MultiPlayMode.PauseAllButThis)
			{
				for (int i = 0; i < _audioMediaPlayers.Count; i++)
					if (i != currentMediaPlayerIndex)
						await _audioMediaPlayers[i].PauseAsync(progressiveVolume);
			}
			else if (multiPlayMode == MultiPlayMode.StopAllButThis)
			{
				for (int i = 0; i < _audioMediaPlayers.Count; i++)
					if (i != currentMediaPlayerIndex)
						await _audioMediaPlayers[i].StopAsync(false, progressiveVolume);
			}
		}

		public List<int> GetPlayingMediaPlayers()
		{
			var result = new List<int>();
			
			int i = 0;
			foreach (var audioMediaPlayer in _audioMediaPlayers)
			{
				if (audioMediaPlayer.MediaPlayer.IsPlaying)
					result.Add(i);

				i++;
			}

			return result;
		}

		public Dictionary<int, IMediaPlayInfo> GetMediaPlayInfo()
		{
			var result = new Dictionary<int, IMediaPlayInfo>();

			int i = 0;
			foreach (var audioMediaPlayer in _audioMediaPlayers)
			{
				result.Add(i, audioMediaPlayer.GetMediaPlayInfo());
				i++;
			}

			return result;
		}
	}
}
