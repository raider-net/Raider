using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Raider.IOUtils
{
	/*
	 USAGE:
		//FULL LIST https://www.filesignatures.net/
		var fileSignatureMap = new Dictionary<string, List<byte[]>>
		{
			{ ".jpg", new List<byte[]>
				{
					new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
					new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
					new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
				}
			},
		};
		var feHelper = new FileExtensionHelper(fileSignatureMap);
		feHelper.IsValidFileExtensionAndSignature("test.jpg", stream, new string[] { "jpg" }, false);
	 */

	public class FileExtensionHelper
	{
		private readonly Dictionary<string, List<byte[]>> _fileSignatureMap;
		private readonly int _maxHeaderBytes;

		private List<byte[]>? _allSignatures;

		public static readonly byte[] PdfSignature = new byte[] { 0x25, 0x50, 0x44, 0x46 };

		public FileExtensionHelper(Dictionary<string, List<byte[]>> fileSignatureMap)
		{
			_fileSignatureMap = fileSignatureMap ?? throw new ArgumentNullException(nameof(fileSignatureMap));
			_maxHeaderBytes = _fileSignatureMap.Max(x => x.Value.Max(b => b.Length));
		}

		public bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[]? permittedExtensions = null, bool defaultWhenExtensionNotFound = false, bool leaveOpen = true)
		{
			if (string.IsNullOrWhiteSpace(fileName) || data == null || data.Length == 0)
				return false;

			var ext = Path.GetExtension(fileName).ToLowerInvariant();
			return IsValidFileExtensionAndSignature(data, ext, permittedExtensions, defaultWhenExtensionNotFound, leaveOpen);
		}

		public bool IsValidFileExtensionAndSignature(Stream data, string fileExtension, string[]? permittedExtensions = null, bool defaultWhenExtensionNotFound = false, bool leaveOpen = true)
		{
			if (string.IsNullOrWhiteSpace(fileExtension) || data == null || data.Length == 0)
				return false;

			var ext = fileExtension.ToLowerInvariant();

			if (!fileExtension.StartsWith("."))
				fileExtension = $".{fileExtension}";

			if (permittedExtensions != null && !permittedExtensions.Contains(ext))
				return defaultWhenExtensionNotFound;

			if (data.Position != 0)
				data.Seek(0, SeekOrigin.Begin);

			if (_fileSignatureMap.TryGetValue(ext, out List<byte[]>? signatures))
			{
				using var reader = new BinaryReader(data, new UTF8Encoding(), leaveOpen);
				var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
				var result = signatures.Any(s => headerBytes.Take(s.Length).SequenceEqual(s));

				if (data.CanSeek)
					data.Seek(0, SeekOrigin.Begin);

				return result;
			}
			else
			{
				return defaultWhenExtensionNotFound;
			}
		}

		public bool HasAllowedSignature(Stream data, bool leaveOpen = true)
		{
			if (data == null || data.Length == 0)
				return false;

			if (data.Position != 0)
				data.Seek(0, SeekOrigin.Begin);

			using var reader = new BinaryReader(data, new UTF8Encoding(), leaveOpen);
			var headerBytes = reader.ReadBytes(_maxHeaderBytes);

			if (_allSignatures == null)
				_allSignatures = _fileSignatureMap.Values.SelectMany(x => x).ToList();

			var result = _allSignatures.Any(s => headerBytes.Take(s.Length).SequenceEqual(s));

			if (data.CanSeek)
				data.Seek(0, SeekOrigin.Begin);

			return result;
		}

		public static bool HasSignature(Stream data, byte[] signature, bool leaveOpen = true)
		{
			if (data == null || data.Length == 0 || signature == null || signature.Length == 0)
				return false;

			if (data.Position != 0)
				data.Seek(0, SeekOrigin.Begin);

			using var reader = new BinaryReader(data, new UTF8Encoding(), leaveOpen);
			var headerBytes = reader.ReadBytes(signature.Length);

			var result = headerBytes.SequenceEqual(signature);

			if (data.CanSeek)
				data.Seek(0, SeekOrigin.Begin);

			return result;
		}
	}
}
