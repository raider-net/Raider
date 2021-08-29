#nullable disable

namespace Raider.Net
{
	public class FtpConfig
	{
		public string HostName { get; set; }

		public int Port { get; set; }

		public bool EnableSsl { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public int? RequestTimeoutInMilliseconds { get; set; }

		public string Validate()
		{
			var prefix = $"Invalid {nameof(FtpConfig)}.";

			if (string.IsNullOrWhiteSpace(HostName))
				return $"{prefix} {nameof(HostName)} == null";

			if (Port < 1)
				Port = 21;

			//if (string.IsNullOrWhiteSpace(UserName))
			//	return $"{prefix} {nameof(UserName)} == null";

			//if (string.IsNullOrWhiteSpace(Password))
			//	return $"{prefix} {nameof(Password)} == null";

			if (RequestTimeoutInMilliseconds < 1)
				RequestTimeoutInMilliseconds = null;

			return null;
		}
	}
}
