namespace Raider.Logging.Database.PostgreSql
{
	public class DbLogWriterConfiguration
	{
		protected EnvironmentInfoWriter? _environmentInfoWriter;
		protected HardwareInfoWriter? _hardwareInfoWriter;

		public DbLogWriterConfiguration SetEnvironmentInfoWriter(DBEnvironmentInfoSinkOptions options)
		{
			_environmentInfoWriter = new EnvironmentInfoWriter(options);
			return this;
		}

		public DbLogWriterConfiguration SetHardwareInfoWriter(DBHardwareInfoSinkOptions options)
		{
			_hardwareInfoWriter = new HardwareInfoWriter(options);
			return this;
		}

		public DbLogWriter? CreateDbLogWriter()
		{
			if (_environmentInfoWriter == null && _hardwareInfoWriter == null)
				return null;

			return new DbLogWriter(_environmentInfoWriter, _hardwareInfoWriter);
		}
	}
}
