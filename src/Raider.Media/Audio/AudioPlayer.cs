using LibVLCSharp.Shared;
using Raider.Extensions;
using Raider.Threading;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Raider.Media.Audio
{
	public class AudioPlayer : IAudioPlayer
	{
		private const int progressiveVolumeTotalDurationInMilliseconds = 1000;
		private const int progressiveVolumeSteps = 5;
		private readonly int progressiveVolumeStepDurationInMilliseconds;

		private readonly LibVLC _libVLC;
		private readonly AsyncLock _audioMediaPlayerLock;
		private readonly AsyncLock _setVolumeLock;
		private readonly StringBuilder _errorBuilder;
		
		public MediaPlayer MediaPlayer { get; }
		public IMediaInfo? MediaInfo { get; private set; }
		public LibVLCSharp.Shared.Media? Media { get; private set; }
		public DateTime? MediaStartTime { get; private set; }
		public DateTime? MediaEndTime { get; private set; }
		public Stopwatch MediaDurationStopwatch { get; }
		public string MediaError => _errorBuilder.ToString();
		public bool MediaManuallyStopped { get; private set; }
		public bool IsPlaying => MediaPlayer.IsPlaying;
		public int TargetVolume { get; private set; }
		
		public event Action<IMediaPlayInfo>? OnStop;
		public event Action<string>? OnError;
		public event Func<IMediaPlayInfo, Task>? OnStopAsync;
		public event Func<string, Task>? OnErrorAsync;

		public AudioPlayer(LibVLC libVLC, int targetVolume = 100)
		{
			_libVLC = libVLC ?? throw new ArgumentNullException(nameof(libVLC));

			progressiveVolumeStepDurationInMilliseconds = progressiveVolumeTotalDurationInMilliseconds / progressiveVolumeSteps;
			_audioMediaPlayerLock = new AsyncLock();
			_setVolumeLock = new AsyncLock();

			MediaPlayer = new MediaPlayer(_libVLC);
			SetTargetVolume(targetVolume);

			_errorBuilder = new StringBuilder();
			MediaDurationStopwatch = new Stopwatch();
			TargetVolume = targetVolume;
			MediaPlayer.Volume = TargetVolume;

			MediaPlayer.Playing += (object? sender, EventArgs e) =>
			{
				if (MediaStartTime.HasValue) //resume
				{
					MediaDurationStopwatch.Start();
				}
				else //playing first time
				{
					MediaStartTime = DateTime.UtcNow;
					MediaDurationStopwatch.Restart();
				}
			};

			MediaPlayer.Paused += (object? sender, EventArgs e) =>
			{
				MediaDurationStopwatch.Stop();
			};

			MediaPlayer.Stopped += async (object? sender, EventArgs e) =>
				{
					if (MediaStartTime.HasValue && !MediaEndTime.HasValue)
						MediaEndTime = DateTime.UtcNow;

					MediaDurationStopwatch.Stop();
					if (MediaInfo != null)
					{
						if (OnStop != null)
						{
							try
							{
								OnStop.Invoke(new MediaPlayInfo(MediaInfo)
								{
									MediaStartTime = MediaStartTime,
									MediaEndTime = MediaEndTime,
									PlayTimeInSeconds = MediaDurationStopwatch.Elapsed.TotalSeconds,
									MediaError = MediaError,
									MediaManuallyStopped = MediaManuallyStopped
								});
							}
							catch { }
						}

						if (OnStopAsync != null)
						{
							try
							{
								await OnStopAsync.Invoke(new MediaPlayInfo(MediaInfo)
								{
									MediaStartTime = MediaStartTime,
									MediaEndTime = MediaEndTime,
									PlayTimeInSeconds = MediaDurationStopwatch.Elapsed.TotalSeconds,
									MediaError = MediaError,
									MediaManuallyStopped = MediaManuallyStopped
								});
							}
							catch { }
						}
					}
				};

			MediaPlayer.EndReached += (object? sender, EventArgs e) =>
			{
				if (MediaStartTime.HasValue && !MediaEndTime.HasValue)
					MediaEndTime = DateTime.UtcNow;

				MediaDurationStopwatch.Stop();
			};
		}

		public void SetTargetVolume(int targetVolume)
		{
			if (targetVolume < 0)
				targetVolume = 0;
			else if (100 < targetVolume)
				targetVolume = 100;

			TargetVolume = targetVolume;
		}

		public async Task SetVolumeAsync(int toVolume, bool progressive)
		{
			using (await _setVolumeLock.LockAsync())
			{
				if (toVolume < 0)
					toVolume = 0;
				else if (100 < toVolume)
					toVolume = 100;

				var mediaPlayerVolume = MediaPlayer.Volume;

				if (toVolume == mediaPlayerVolume)
					return;

				if (progressive)
				{
					if (toVolume < mediaPlayerVolume)
					{
						var diff = mediaPlayerVolume - toVolume;
						var vol = diff / progressiveVolumeSteps;
						for (int i = 0; i < (progressiveVolumeSteps - 1); i++)
						{
							MediaPlayer.Volume -= vol;
							await Task.Delay(progressiveVolumeStepDurationInMilliseconds);
						}
					}
					else //if (mediaPlayerVolume < targetVolume)
					{
						var diff = toVolume - mediaPlayerVolume;
						var vol = diff / progressiveVolumeSteps;
						for (int i = 0; i < (progressiveVolumeSteps - 1); i++)
						{
							MediaPlayer.Volume += vol;
							await Task.Delay(progressiveVolumeStepDurationInMilliseconds);
						}
					}
				}

				MediaPlayer.Volume = toVolume;
			}
		}

		private void ResetPaying()
		{
			MediaManuallyStopped = false;
			MediaStartTime = null;
			MediaEndTime = null;
		}

		public async Task<bool> PlayAsync(IMediaInfo mediaInfo, int? volume, bool progressiveVolume)
		{
			if (mediaInfo == null)
				throw new ArgumentNullException(nameof(mediaInfo));

			if (string.IsNullOrWhiteSpace(mediaInfo.MediaLocation))
				throw new ArgumentException($"{nameof(mediaInfo)}.{nameof(mediaInfo.MediaLocation)} == null", $"{nameof(mediaInfo)}.{nameof(mediaInfo.MediaLocation)}");

			MediaInfo = mediaInfo;

			using (await _audioMediaPlayerLock.LockAsync())
			{
				try
				{
					if (IsPlaying)
						await StopInternalAsync(true, progressiveVolume);

					if (volume.HasValue)
						SetTargetVolume(volume.Value);

					if (progressiveVolume)
					{
						MediaPlayer.Volume = 0;
					}
					else
					{
						MediaPlayer.Volume = TargetVolume;
					}

					ResetPaying();

					Media = new LibVLCSharp.Shared.Media(_libVLC, mediaInfo.MediaLocation);
					var result = MediaPlayer.Play(Media);

					if (progressiveVolume)
						await SetVolumeAsync(TargetVolume, true);

					return result;
				}
				catch (Exception ex)
				{
					await WriteErrorLogAsync("PLAY", ex);
					return false;
				}
			}
		}

		public async Task<double?> PauseAsync(bool progressiveVolume)
		{
			if (!IsPlaying)
				return null;

			using (await _audioMediaPlayerLock.LockAsync())
			{
				try
				{
					if (!IsPlaying)
						return null;

					if (progressiveVolume)
						await SetVolumeAsync(0, true);

					MediaPlayer.Pause();

					return MediaDurationStopwatch.Elapsed.TotalSeconds;
				}
				catch (Exception ex)
				{
					await WriteErrorLogAsync("PAUSE", ex);
					return null;
				}
			}
		}

		public async Task<bool> ResumeAsync(int? volume, bool progressiveVolume)
		{
			if (IsPlaying)
				return true;

			using (await _audioMediaPlayerLock.LockAsync())
			{
				try
				{
					if (IsPlaying)
						return true;

					if (volume.HasValue)
						SetTargetVolume(volume.Value);

					if (progressiveVolume)
					{
						MediaPlayer.Volume = 0;
					}
					else
					{
						MediaPlayer.Volume = TargetVolume;
					}

					var result = MediaPlayer.Play();

					if (progressiveVolume)
						await SetVolumeAsync(TargetVolume, true);

					return result;
				}
				catch (Exception ex)
				{
					await WriteErrorLogAsync("RESUME", ex);
					return false;
				}
			}
		}

		public async Task<double?> StopAsync(bool removeMedia, bool progressiveVolume)
		{
			if (!IsPlaying)
			{
				if (removeMedia)
					Media = null;

				return null;
			}

			using (await _audioMediaPlayerLock.LockAsync())
			{
				return await StopInternalAsync(removeMedia, progressiveVolume);
			}
		}

		private async Task<double?> StopInternalAsync(bool removeMedia, bool progressiveVolume)
		{
			if (!IsPlaying)
			{
				if (removeMedia)
					Media = null;

				return null;
			}

			try
			{
				if (progressiveVolume)
					await SetVolumeAsync(0, true);

				MediaManuallyStopped = true;
				MediaPlayer.Stop();

				if (removeMedia)
					Media = null;

				return MediaDurationStopwatch.Elapsed.TotalSeconds;
			}
			catch (Exception ex)
			{
				await WriteErrorLogAsync("STOP", ex);
				return null;
			}
		}

		private async Task WriteErrorLogAsync(string methodName, Exception ex)
		{
			if (ex == null)
				return;

			var sb = 
				new StringBuilder()
					.Append(DateTime.UtcNow.ToUtcFormat()).Append(" [").Append(methodName).Append("]: ").AppendLine(ex.Message)
					.Append(ex.ToString());

			_errorBuilder.Append(sb).AppendLine();

			if (OnError != null)
			{
				try
				{
					OnError.Invoke(sb.ToString());
				}
				catch { }
			}

			if (OnErrorAsync != null)
			{
				try
				{
					await OnErrorAsync.Invoke(sb.ToString());
				}
				catch { }
			}
		}
	}
}
