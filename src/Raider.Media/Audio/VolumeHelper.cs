using System;

namespace Raider.Media.Audio
{
	public static class VolumeHelper
	{
		private const double thirdRoof = 1.0 / 3.0;

		public static int ThirdRoofOfScaleOnAmplitude(int targetVolume)
		{
			if (targetVolume <= 0)
				return 0;

			if (100 <= targetVolume)
				return 100;

			var targetPercent = targetVolume / 100.0;
			var volume = Math.Pow(targetPercent, thirdRoof) * 100.0;
			return Convert.ToInt32(Math.Round(volume, 0));
		}

		public static int CubicScaleOnAmplitude(int volume)
		{
			if (volume <= 0)
				return 0;

			if (100 <= volume)
				return 100;

			var percent = volume / 100.0;
			var targetVolume = Math.Pow(percent, 3) * 100.0;
			return Convert.ToInt32(Math.Round(targetVolume, 0));
		}
	}
}
