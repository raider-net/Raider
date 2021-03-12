using NpgsqlTypes;
using Raider.AspNetCore.Logging.Dto;
using Raider.Data;
using Raider.Database.PostgreSql;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	public class DBResponseSinkOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public DBResponseSinkOptions()
		{
			TableName = nameof(Response);

			PropertyNames = new List<string>
			{
				nameof(Response.RuntimeUniqueKey),
				nameof(Response.Created),
				nameof(Response.CorrelationId),
				nameof(Request.ExternalCorrelationId),
				nameof(Response.StatusCode),
				nameof(Response.Headers),
				nameof(Response.Body),
				nameof(Response.BodyByteArray),
				nameof(Response.Error),
				nameof(Response.ElapsedMilliseconds)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(Response.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(Response.Created), NpgsqlDbType.TimestampTz },
				{ nameof(Response.CorrelationId), NpgsqlDbType.Uuid },
				{ nameof(Request.ExternalCorrelationId), NpgsqlDbType.Varchar },
				{ nameof(Response.StatusCode), NpgsqlDbType.Integer },
				{ nameof(Response.Headers), NpgsqlDbType.Varchar },
				{ nameof(Response.Body), NpgsqlDbType.Varchar },
				{ nameof(Response.BodyByteArray), NpgsqlDbType.Bytea },
				{ nameof(Response.Error), NpgsqlDbType.Varchar },
				{ nameof(Response.ElapsedMilliseconds), NpgsqlDbType.Numeric }
			};
		}
	}
}
