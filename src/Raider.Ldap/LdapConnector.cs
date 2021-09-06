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
		private readonly UTF8Encoding _utf8;

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

			_utf8 = new UTF8Encoding(false, true);
		}

		public LdapSearchResult Search(LdapSearchConfig search)
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

			var result = new LdapSearchResult();
			var pages = 0;
			while (true)
			{
				pages++;

				SearchResponse? searchResponse;
				try
				{
					searchResponse = _connection.SendRequest(searchRequest) as SearchResponse;
					ParseEntries(searchResponse, result);
				}
				catch (DirectoryOperationException ex)
				{
					result.Errors.Add(ex.Message);
					searchResponse = ex.Response as SearchResponse;
					ParseEntries(searchResponse, result);
				}

				if (searchResponse == null)
					break;

				if (pageResultRequestControl == null)
					break;

				if (searchResponse.Controls.Length == 0)
					break;

				if (searchResponse.Controls[0] is not PageResultResponseControl pageResponseControl)
					break;

				if (pageResponseControl.Cookie.Length == 0)
					break;

				pageResultRequestControl.Cookie = pageResponseControl.Cookie;
			}

			return result;
		}

		private void ParseEntries(SearchResponse? searchResponse, LdapSearchResult result)
		{
			if (searchResponse == null)
				return;

			foreach (SearchResultEntry entry in searchResponse.Entries)
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
								attributeValues.Values.Add(new LdapValue { StringValue = _utf8.GetString(v) });
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
				result.AddEntry(new LdapEntry(attributes));
			}
		}
	}
}
