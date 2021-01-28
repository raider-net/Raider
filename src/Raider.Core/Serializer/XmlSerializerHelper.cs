using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Raider.Extensions;

namespace Raider.Serializer
{
	public static class XmlSerializerHelper
	{
		private static XmlSerializer GetXmlSerializer(
			Type type,
			XmlAttributeOverrides overrides,
			Type[] extraTypes,
			XmlRootAttribute root,
			string defaultNamespace,
			string location)
		{
			if (type == null)
				throw new System.ArgumentNullException(nameof(type));

			XmlSerializer serializer = null;

			if (string.IsNullOrWhiteSpace(defaultNamespace)) defaultNamespace = null;
			if (string.IsNullOrWhiteSpace(location)) location = null;

			if (overrides == null && extraTypes == null && root == null && defaultNamespace == null && location == null)
			{
				serializer = new XmlSerializer(type);
			}
			else
			if (overrides != null && extraTypes == null && root == null && defaultNamespace == null && location == null)
			{
				serializer = new XmlSerializer(type, overrides);
			}
			else
			if (overrides == null && extraTypes != null && root == null && defaultNamespace == null && location == null)
			{
				serializer = new XmlSerializer(type, extraTypes);
			}
			else
			if (overrides == null && extraTypes == null && root != null && defaultNamespace == null && location == null)
			{
				serializer = new XmlSerializer(type, root);
			}
			else
			if (overrides == null && extraTypes == null && root == null && defaultNamespace != null && location == null)
			{
				serializer = new XmlSerializer(type, defaultNamespace);
			}
			else
			{
				serializer = new XmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);
			}

			return serializer;
		}

		public static object ReadFromXml(string xmlFilePath,
										Type type,
										Encoding encoding = null,
										bool unzipXml = false,
										FileAccess fileAccess = FileAccess.Read,
										FileShare fileSahre = FileShare.ReadWrite,
										XmlAttributeOverrides overrides = null,
										Type[] extraTypes = null,
										XmlRootAttribute root = null,
										string defaultNamespace = null,
										string location = null)
		{
			if (string.IsNullOrWhiteSpace(xmlFilePath))
				throw new ArgumentNullException(nameof(xmlFilePath));

			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (encoding == null)
				encoding = Encoding.UTF8;
			//else
			//	Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

			using (var fileStream = new FileStream(xmlFilePath, FileMode.Open, fileAccess, fileSahre))
			{
				if (unzipXml)
				{
					using (var gZipStream = new GZipStream(fileStream, CompressionMode.Decompress, false))
					using (var textReader = new StreamReader(gZipStream, encoding))
					{
						object objectFromXml = serializer.Deserialize(textReader);
						return objectFromXml;
					}
				}
				else
				{
					using (var textReader = new StreamReader(fileStream, encoding))
					{
						object objectFromXml = serializer.Deserialize(textReader);
						return objectFromXml;
					}
				}
			}
		}

		public static T ReadFromXml<T>(string xmlFilePath,
										Encoding encoding = null,
										bool unzipXml = false,
										FileAccess fileAccess = FileAccess.Read,
										FileShare fileSahre = FileShare.ReadWrite,
										XmlAttributeOverrides overrides = null,
										Type[] extraTypes = null,
										XmlRootAttribute root = null,
										string defaultNamespace = null,
										string location = null)
		{
			object result = ReadFromXml(xmlFilePath, typeof(T), encoding, unzipXml, fileAccess, fileSahre, overrides, extraTypes, root, defaultNamespace, location);
			return result == null
				? default(T)
				: (T)result;
		}

		public static object ReadFromString(string xmlObject,
											Type type,
											Encoding encoding = null,
											XmlAttributeOverrides overrides = null,
											Type[] extraTypes = null,
											XmlRootAttribute root = null,
											string defaultNamespace = null,
											string location = null)
		{
			if (string.IsNullOrWhiteSpace(xmlObject))
				return null;

			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (encoding == null)
				encoding = Encoding.UTF8;
			//else
			//	Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

			using (var memoryStream = xmlObject.ToMemoryStream(encoding))
			using (var xmlReader = XmlReader.Create(memoryStream))
			{
				object objectFromXml = serializer.Deserialize(xmlReader);
				return objectFromXml;
			}
		}

		public static T ReadFromString<T>(string xmlObject,
											Encoding encoding = null,
											XmlAttributeOverrides overrides = null,
											Type[] extraTypes = null,
											XmlRootAttribute root = null,
											string defaultNamespace = null,
											string location = null)
		{
			object result = ReadFromString(xmlObject, typeof(T), encoding, overrides, extraTypes, root, defaultNamespace, location);
			return result == null
				? default(T)
				: (T)result;
		}

		public static object ReadFromByteArray(byte[] xmlObject,
												Type type,
												bool unzipXml = false,
												XmlAttributeOverrides overrides = null,
												Type[] extraTypes = null,
												XmlRootAttribute root = null,
												string defaultNamespace = null,
												string location = null)
		{
			if (xmlObject == null || xmlObject.Length == 0)
				return null;

			if (type == null)
				throw new ArgumentNullException(nameof(type));

			using (var memoryStream = new MemoryStream(xmlObject))
			{
				object objectFromXml = ReadFromStream(memoryStream, type, unzipXml, overrides, extraTypes, root, defaultNamespace, location);
				return objectFromXml;
			}
		}

		public static T ReadFromByteArray<T>(byte[] xmlObject,
											bool unzipXml = false,
											XmlAttributeOverrides overrides = null,
											Type[] extraTypes = null,
											XmlRootAttribute root = null,
											string defaultNamespace = null,
											string location = null)
		{
			object result = ReadFromByteArray(xmlObject, typeof(T), unzipXml, overrides, extraTypes, root, defaultNamespace, location);
			return result == null
				? default(T)
				: (T)result;
		}

		public static object ReadFromStream(Stream xmlStream,
												Type type,
												bool unzipXml = false,
												XmlAttributeOverrides overrides = null,
												Type[] extraTypes = null,
												XmlRootAttribute root = null,
												string defaultNamespace = null,
												string location = null)
		{
			if (xmlStream == null)
				return null;

			if (type == null)
				throw new ArgumentNullException(nameof(type));

			var serializer = GetXmlSerializer(type, overrides, extraTypes, root, defaultNamespace, location);

			if (unzipXml)
			{
				using (var tmpStream = new MemoryStream())
				using (var gzipStream = new GZipStream(xmlStream, CompressionMode.Decompress, false))
				{
					gzipStream.BlockCopy(tmpStream);
					tmpStream.Seek(0, SeekOrigin.Begin);
					object objectFromXml = serializer.Deserialize(tmpStream);
					return objectFromXml;
				}
			}
			else
			{
				object objectFromXml = serializer.Deserialize(xmlStream);
				return objectFromXml;
			}
		}

		public static T ReadFromStream<T>(Stream xmlStream,
											bool unzipXml = false,
											XmlAttributeOverrides overrides = null,
											Type[] extraTypes = null,
											XmlRootAttribute root = null,
											string defaultNamespace = null,
											string location = null)
		{
			object result = ReadFromStream(xmlStream, typeof(T), unzipXml, overrides, extraTypes, root, defaultNamespace, location);
			return result == null
				? default(T)
				: (T)result;
		}

		/// <summary>
		/// Can not serialize circular references.
		/// Use <see cref="XmlDataContractSerializerHelper"/> or <see cref="JsonSerializerHelper"/> instead.
		/// </summary>
		public static void WriteToXml(string xmlFilePath,
										object obj,
										Encoding encoding = null,
										bool zipXml = false,
										XmlAttributeOverrides overrides = null,
										Type[] extraTypes = null,
										XmlRootAttribute root = null,
										string defaultNamespace = null,
										string location = null)
		{
			if (string.IsNullOrWhiteSpace(xmlFilePath))
				throw new ArgumentNullException(nameof(xmlFilePath));

			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			if (encoding == null)
				encoding = Encoding.UTF8;
			//else
			//	Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			string dir = Path.GetDirectoryName(xmlFilePath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var serializer = GetXmlSerializer(obj.GetType(), overrides, extraTypes, root, defaultNamespace, location);

			using (var fileStream = new FileStream(xmlFilePath, FileMode.Create))
			{
				if (zipXml)
				{
					using (var gZipStream = new GZipStream(fileStream, CompressionMode.Compress, false))
					using (var textWriter = new StreamWriter(gZipStream, encoding))
					{
						serializer.Serialize(textWriter, obj);
					}
				}
				else
				{
					using (var textWriter = new StreamWriter(fileStream, encoding))
					{
						serializer.Serialize(textWriter, obj);
					}
				}
			}
		}

		/// <summary>
		/// Can not serialize circular references.
		/// Use <see cref="XmlDataContractSerializerHelper"/> or <see cref="JsonSerializerHelper"/> instead.
		/// </summary>
		public static string WriteToString(object obj,
											Encoding encoding = null,
											XmlAttributeOverrides overrides = null,
											Type[] extraTypes = null,
											XmlRootAttribute root = null,
											string defaultNamespace = null,
											string location = null,
											bool indent = false)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			if (encoding == null)
				encoding = Encoding.UTF8;
			//else
			//	Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var serializer = GetXmlSerializer(obj.GetType(), overrides, extraTypes, root, defaultNamespace, location);

			using (var textWriter = new StringWriter())
			{
				var settings = new XmlWriterSettings()
				{
					Encoding = encoding, // new UnicodeEncoding(false, false); // no BOM in a .NET string
					Indent = indent,
					OmitXmlDeclaration = false
				};
				using (var xmlWriter = XmlWriter.Create(textWriter, settings))
				{
					serializer.Serialize(xmlWriter, obj);
				}
				string result = textWriter.ToString();
				result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"" + encoding.WebName + "\"?>");
				return result;
			}
		}

		/// <summary>
		/// Can not serialize circular references.
		/// Use <see cref="XmlDataContractSerializerHelper"/> or <see cref="JsonSerializerHelper"/> instead.
		/// </summary>
		public static XmlDocument WriteToXmlDocument(object obj,
											XmlAttributeOverrides overrides = null,
											Type[] extraTypes = null,
											XmlRootAttribute root = null,
											string defaultNamespace = null,
											string location = null)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			XmlDocument xmlDocument = new XmlDocument();
			using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
			{
				var serializer = GetXmlSerializer(obj.GetType(), overrides, extraTypes, root, defaultNamespace, location);
				serializer.Serialize(writer, obj);
			}
			return xmlDocument;
		}

		/// <summary>
		/// Can not serialize circular references.
		/// Use <see cref="XmlDataContractSerializerHelper"/> or <see cref="JsonSerializerHelper"/> instead.
		/// </summary>
		public static XmlElement WriteToXmlElement(object obj,
											XmlAttributeOverrides overrides = null,
											Type[] extraTypes = null,
											XmlRootAttribute root = null,
											string defaultNamespace = null,
											string location = null)
		{
			return WriteToXmlDocument(obj, overrides, extraTypes, root, defaultNamespace, location).DocumentElement;
		}

		/// <summary>
		/// Can not serialize circular references.
		/// Use <see cref="XmlDataContractSerializerHelper"/> or <see cref="JsonSerializerHelper"/> instead.
		/// </summary>
		public static byte[] WriteToByteArray(object obj,
												Encoding encoding = null,
												bool zipXml = false,
												XmlAttributeOverrides overrides = null,
												Type[] extraTypes = null,
												XmlRootAttribute root = null,
												string defaultNamespace = null,
												string location = null)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			using (MemoryStream memoryStream = WriteToStream(obj, encoding, zipXml, overrides, extraTypes, root, defaultNamespace, location))
			{
				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// Can not serialize circular references.
		/// Use <see cref="XmlDataContractSerializerHelper"/> or <see cref="JsonSerializerHelper"/> instead.
		/// </summary>
		public static MemoryStream WriteToStream(object obj,
													Encoding encoding = null,
													bool zipXml = false,
													XmlAttributeOverrides overrides = null,
													Type[] extraTypes = null,
													XmlRootAttribute root = null,
													string defaultNamespace = null,
													string location = null)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			if (encoding == null)
				encoding = Encoding.UTF8;

			var serializer = GetXmlSerializer(obj.GetType(), overrides, extraTypes, root, defaultNamespace, location);

			var memoryStream = new MemoryStream();
			var memoryStreamWriter = new StreamWriter(memoryStream, encoding);

			if (zipXml)
			{
				using (var tmpStream = new MemoryStream())
				{
					serializer.Serialize(tmpStream, obj);
					tmpStream.Seek(0, SeekOrigin.Begin);
					using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
					{
						tmpStream.BlockCopy(gzipStream);
					}
				}
			}
			else
			{
				serializer.Serialize(memoryStreamWriter, obj);
			}

			memoryStream.Seek(0, SeekOrigin.Begin);
			//memoryStream.Seek(0, SeekOrigin.Begin);
			return memoryStream;
		}
	}
}
