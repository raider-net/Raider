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

		public List<LdapEntry> Search(LdapSearchConfig search)
		{
			if (search == null)
				throw new ArgumentNullException(nameof(search));

			var searchRequest = new SearchRequest(search.DistinguishedName, search.LdapFilter, search.SearchScope, search.Attributes?.ToArray());

			PageResultRequestControl? pageResultRequestControl = null;
			if (search.MaxResultsToRequest.HasValue)
			{
				pageResultRequestControl = new PageResultRequestControl(search.MaxResultsToRequest.Value);
				searchRequest.Controls.Add(pageResultRequestControl);
			}

			if (search.SearchOptionsControl.HasValue)
			{
				var searchOptionsControl = new SearchOptionsControl(search.SearchOptionsControl.Value);
				searchRequest.Controls.Add(searchOptionsControl);
			}

			var utf8 = new UTF8Encoding(false, true);
			var result = new List<LdapEntry>();
			SearchResponse? response = null;

			PageResultResponseControl? pageResponseControl = null;

			var pages = 0;
			while (true)
			{
				pages++;
				response = (SearchResponse)_connection.SendRequest(searchRequest);

				foreach (SearchResultEntry entry in response.Entries)
				{
					var attributes = new List<LdapAttributeValues>();
					foreach (string attrName in entry.Attributes.AttributeNames)
					{
						var attr = entry.Attributes[attrName];
						var attributeValues = new LdapAttributeValues(attr.Name);
						attributes.Add(attributeValues);

						foreach (var o in attr)
						{
							if (o is byte[] v)
							{
								try
								{
									attributeValues.Values.Add(new LdapValue { StringValue = utf8.GetString(v) });
								}
								catch (ArgumentException)
								{
									attributeValues.Values.Add(new LdapValue { ByteArrayValue = v });
								}
							}
							else
							{
								attributeValues.Values.Add(new LdapValue { Error = $"Not supported type: {o?.GetType()?.FullName}" });
							}
						}
					}

					attributes = attributes.OrderBy(x => x.Name).ToList();
					result.Add(new LdapEntry(attributes));
				}

				if (pageResultRequestControl == null)
					break;

				if (response.Controls.Length == 0)
					break;

				pageResponseControl = response.Controls[0] as PageResultResponseControl;
				if (pageResponseControl == null)
					break;

				if (pageResponseControl.Cookie.Length == 0)
					break;

				pageResultRequestControl.Cookie = pageResponseControl.Cookie;
			}

			return result;
		}
	}
}
