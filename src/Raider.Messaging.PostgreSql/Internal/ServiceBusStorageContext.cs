using Npgsql;
using System;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql
{
	internal class ServiceBusStorageContext : IServiceBusStorageContext
	{
		private bool disposedValue;
		public NpgsqlConnection Connection { get; }

		public ServiceBusStorageContext(NpgsqlConnection connection)
		{
			Connection = connection;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Connection.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public ValueTask DisposeAsync()
			=> Connection.DisposeAsync();
	}
}
