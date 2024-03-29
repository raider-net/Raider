﻿using LibVLCSharp.Shared;
using Raider.Extensions;
using Raider.Media.Internal;
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
		public IMediaFile? MediaFile { get; private set; }
		public LibVLCSharp.Shared.Media? Media { get; private set; }
		public DateTime? MediaStartTime { get; private set; }
		public DateTime? MediaEndTime { get; private set; }
		public Stopwatch MediaDurationStopwatch { get; }
		public string MediaError => _errorBuilder.ToString();
		public bool MediaManuallyStopped { get; private set; }
		public bool IsPlaying => MediaPlayer.IsPlaying;
		public int TargetVolume { get; private set; }

		public event Action<IMediaInfo>? OnPlaying;
		public event Action<IMediaInfo>? OnPaused;
		public event Action<IMediaInfo>? OnStop;
		public event Action<string>? OnError;
		public event Func<IMediaInfo, Task>? OnPlayingAsync;
		public event Func<IMediaInfo, Task>? OnPausedAsync;
		public event Func<IMediaInfo, Task>? OnStopAsync;
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
			MediaPlayer.Volume = VolumeHelper.ThirdRoofOfScaleOnAmplitude(TargetVolume);

			MediaPlayer.Playing += async (object? sender, EventArgs e) =>
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

				if (MediaFile != null)
				{
					if (OnPlaying != null)
					{
						try
						{
							OnPlaying.Invoke(GetMediaPlayInfo());
						}
						catch { }
					}

					if (OnPlayingAsync != null)
					{
						try
						{
							await OnPlayingAsync.Invoke(GetMediaPlayInfo());
						}
						catch { }
					}
				}
			};

			MediaPlayer.Paused += async (object? sender, EventArgs e) =>
			{
				MediaDurationStopwatch.Stop();

				if (MediaFile != null)
				{
					if (OnPaused != null)
					{
						try
						{
							OnPaused.Invoke(GetMediaPlayInfo());
						}
						catch { }
					}

					if (OnPausedAsync != null)
					{
						try
						{
							await OnPausedAsync.Invoke(GetMediaPlayInfo());
						}
						catch { }
					}
				}
			};

			MediaPlayer.Stopped += async (object? sender, EventArgs e) =>
				{
					MediaDurationStopwatch.Stop();

					if (MediaStartTime.HasValue && !MediaEndTime.HasValue)
						MediaEndTime = DateTime.UtcNow;

					if (MediaFile != null)
					{
						if (OnStop != null)
						{
							try
							{
								OnStop.Invoke(GetMediaPlayInfo());
							}
							catch { }
						}

						if (OnStopAsync != null)
						{
							try
							{
								await OnStopAsync.Invoke(GetMediaPlayInfo());
							}
							catch { }
						}
					}
				};

			MediaPlayer.EndReached += (object? sender, EventArgs e) =>
			{
				MediaDurationStopwatch.Stop();

				if (MediaStartTime.HasValue && !MediaEndTime.HasValue)
					MediaEndTime = DateTime.UtcNow;
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

		public async Task SetVolumeAsync(int fromVolume, int toVolume, bool progressive)
		{
			using (await _setVolumeLock.LockAsync())
			{
				if (fromVolume < 0)
					fromVolume = 0;
				else if (100 < fromVolume)
					fromVolume = 100;

				if (toVolume < 0)
					toVolume = 0;
				else if (100 < toVolume)
					toVolume = 100;

				if (fromVolume == toVolume)
					return;

				if (progressive)
				{
					if (toVolume < fromVolume)
					{
						var diff = fromVolume - toVolume;
						var vol = diff / progressiveVolumeSteps;
						for (int i = 0; i < (progressiveVolumeSteps - 1); i++)
						{
							var next = VolumeHelper.CubicScaleOnAmplitude(MediaPlayer.Volume) - vol;
							MediaPlayer.Volume = VolumeHelper.ThirdRoofOfScaleOnAmplitude(next);
							await Task.Delay(progressiveVolumeStepDurationInMilliseconds);
						}
					}
					else //if (mediaPlayerVolume < targetVolume)
					{
						var diff = toVolume - fromVolume;
						var vol = diff / progressiveVolumeSteps;
						for (int i = 0; i < (progressiveVolumeSteps - 1); i++)
						{
							var next = VolumeHelper.CubicScaleOnAmplitude(MediaPlayer.Volume) + vol;
							MediaPlayer.Volume = VolumeHelper.ThirdRoofOfScaleOnAmplitude(next);
							await Task.Delay(progressiveVolumeStepDurationInMilliseconds);
						}
					}
				}

				MediaPlayer.Volume = VolumeHelper.ThirdRoofOfScaleOnAmplitude(toVolume);
			}
		}

		private void ResetPaying()
		{
			MediaManuallyStopped = false;
			MediaStartTime = null;
			MediaEndTime = null;
		}

		public async Task<bool> PlayAsync(IMediaFile mediaFile, int? volume, bool progressiveVolume)
		{
			if (mediaFile == null)
				throw new ArgumentNullException(nameof(mediaFile));

			if (string.IsNullOrWhiteSpace(mediaFile.MediaLocation))
				throw new ArgumentException($"{nameof(mediaFile)}.{nameof(mediaFile.MediaLocation)} == null", $"{nameof(mediaFile)}.{nameof(mediaFile.MediaLocation)}");

			MediaFile = mediaFile;

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
						MediaPlayer.Volume = VolumeHelper.ThirdRoofOfScaleOnAmplitude(TargetVolume);
					}

					ResetPaying();

					Media = new LibVLCSharp.Shared.Media(_libVLC, mediaFile.MediaLocation);
					var result = MediaPlayer.Play(Media);

					if (progressiveVolume)
						await SetVolumeAsync(0, TargetVolume, true);

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
						await SetVolumeAsync(VolumeHelper.CubicScaleOnAmplitude(MediaPlayer.Volume), 0, true);

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
						MediaPlayer.Volume = VolumeHelper.ThirdRoofOfScaleOnAmplitude(TargetVolume);
					}

					var result = MediaPlayer.Play();

					if (progressiveVolume)
						await SetVolumeAsync(0, TargetVolume, true);

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
					await SetVolumeAsync(VolumeHelper.CubicScaleOnAmplitude(MediaPlayer.Volume), 0, true);

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

		public IMediaInfo GetMediaPlayInfo()
			=> new MediaInfo(MediaFile!)
			{
				IsPlaying = MediaPlayer.IsPlaying,
				Position = MediaPlayer.Position,
				MediaStartTime = MediaStartTime,
				MediaEndTime = MediaEndTime,
				PlayTimeInSeconds = MediaDurationStopwatch.Elapsed.TotalSeconds,
				Length = MediaPlayer.Length,
				Time = MediaPlayer.Time,
				Volume = VolumeHelper.CubicScaleOnAmplitude(MediaPlayer.Volume),
				Mrl = Media?.Mrl,
				State = Media?.State,
				Type = Media?.Type,
				MediaError = null,
				MediaManuallyStopped = null
			};
	}
}
