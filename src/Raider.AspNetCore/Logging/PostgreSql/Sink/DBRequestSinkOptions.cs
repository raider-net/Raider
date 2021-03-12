using NpgsqlTypes;
using Raider.AspNetCore.Logging.Dto;
using Raider.Data;
using Raider.Database.PostgreSql;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	public class DBRequestSinkOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public DBRequestSinkOptions()
		{
			TableName = nameof(Request);

			PropertyNames = new List<string>
			{
				nameof(Request.RuntimeUniqueKey),
				nameof(Request.Created),
				nameof(Request.CorrelationId),
				nameof(Request.ExternalCorrelationId),
				nameof(Request.Protocol),
				nameof(Request.Scheme),
				nameof(Request.Host),
				nameof(Request.Method),
				nameof(Request.Path),
				nameof(Request.QueryString),
				nameof(Request.Headers),
				nameof(Request.Body),
				nameof(Request.BodyByteArray),
				nameof(Request.Form),
				nameof(Request.Files)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(Request.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(Request.Created), NpgsqlDbType.TimestampTz },
				{ nameof(Request.CorrelationId), NpgsqlDbType.Uuid },
				{ nameof(Request.ExternalCorrelationId), NpgsqlDbType.Varchar },
				{ nameof(Request.Protocol), NpgsqlDbType.Varchar },
				{ nameof(Request.Scheme), NpgsqlDbType.Varchar },
				{ nameof(Request.Host), NpgsqlDbType.Varchar },
				{ nameof(Request.Method), NpgsqlDbType.Varchar },
				{ nameof(Request.Path), NpgsqlDbType.Varchar },
				{ nameof(Request.QueryString), NpgsqlDbType.Varchar },
				{ nameof(Request.Headers), NpgsqlDbType.Varchar },
				{ nameof(Request.Body), NpgsqlDbType.Varchar },
				{ nameof(Request.BodyByteArray), NpgsqlDbType.Bytea },
				{ nameof(Request.Form), NpgsqlDbType.Varchar },
				{ nameof(Request.Files), NpgsqlDbType.Varchar }
			};
		}
	}
}
