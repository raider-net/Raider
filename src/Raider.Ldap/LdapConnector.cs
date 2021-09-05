using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Raider.Ldap
{
	public class LdapConnector
	{
		private readonly LdapConnection _connection;

		public LdapConnector(LdapConnectorConfig config)
		{
			if (config == null)
				throw new ArgumentNullException(nameof(config));

			var validationError = config.Validate();
			if (!string.IsNullOrWhiteSpace(validationError))
				throw new ArgumentException(validationError, nameof(config));

			_connection = new LdapConnection(new LdapDirectoryIdentifier(config.Server, config.Port, false, false))
			{
				AuthType = config.AuthType,
			};
			_connection.Timeout = TimeSpan.FromSeconds(30);
			_connection.SessionOptions.VerifyServerCertificate = (LdapConnection conn, X509Certificate cert) => true;
			_connection.SessionOptions.SecureSocketLayer = config.SecureSocketLayer;

			if (string.IsNullOrWhiteSpace(config.UserName))
			{
				_connection.Bind();
			}
			else
			{
				var credential = new NetworkCredential(config.UserName, config.Password, config.DomainName);
				_connection.Bind(credential);
			}
		}

		public List<LdapResultValue> Search(LdapSearchConfig search)
		{
			if (search == null)
				throw new ArgumentNullException(nameof(search));

			var result = new List<LdapResultValue>();
			var searchRequest = new SearchRequest(search.DistinguishedName, search.LdapFilter, search.SearchScope, search.Attributes?.ToArray());
			
			var response = (SearchResponse)_connection.SendRequest(searchRequest);
			var utf8 = new UTF8Encoding(false, true);

			foreach (SearchResultEntry entry in response.Entries)
			{
				foreach (string attrName in entry.Attributes.AttributeNames)
				{
					var attr = entry.Attributes[attrName];
					var val = new LdapResultValue(attr.Name);
					result.Add(val);

					foreach (var o in attr)
					{
						if (o is byte[] v)
						{
							try
							{
								val.Values.Add(new LdapValue { StringValue = utf8.GetString(v) });
							}
							catch (ArgumentException)
							{
								val.Values.Add(new LdapValue { ByteArrayValue = v });
							}
						}
						else
						{
							val.Values.Add(new LdapValue { Error = $"Not supported type: {o?.GetType()?.FullName}" });
						}
					}
				}
			}

			result = result.OrderBy(x => x.Name).ToList();
			return result;
		}
	}
}
