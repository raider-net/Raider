namespace Raider.Logging.Internal
{
	internal class SilentLogWriter : ILogWriter
	{
		public static readonly ILogWriter Instance = new SilentLogWriter();

		public void Write<T>(T obj) { }

		public void Dispose() { }
	}
}
