using NpgsqlTypes;
using Raider.AspNetCore.Logging.Dto;
using Raider.Data;
using Raider.Database.PostgreSql;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	public class DBRequestAuthenticationSinkOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public DBRequestAuthenticationSinkOptions()
		{
			TableName = nameof(RequestAuthentication);

			PropertyNames = new List<string>
			{
				nameof(RequestAuthentication.RuntimeUniqueKey),
				nameof(RequestAuthentication.Created),
				nameof(RequestAuthentication.CorrelationId),
				nameof(RequestAuthentication.ExternalCorrelationId),
				nameof(RequestAuthentication.IdUser),
				nameof(RequestAuthentication.Roles),
				nameof(RequestAuthentication.Permissions)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(RequestAuthentication.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(RequestAuthentication.Created), NpgsqlDbType.TimestampTz },
				{ nameof(RequestAuthentication.CorrelationId), NpgsqlDbType.Uuid },
				{ nameof(RequestAuthentication.ExternalCorrelationId), NpgsqlDbType.Varchar },
				{ nameof(RequestAuthentication.IdUser), NpgsqlDbType.Integer },
				{ nameof(RequestAuthentication.Roles), NpgsqlDbType.Varchar },
				{ nameof(RequestAuthentication.Permissions), NpgsqlDbType.Varchar }
			};
		}
	}
}
