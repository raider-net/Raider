using Raider.Extensions;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Net
{
	public class FtpClient : IAsyncFtpClient
	{
		private readonly FtpConfig _config;
		public string BaseUri { get; }

		public FtpClient(FtpConfig config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			var configValidation = _config.Validate();
			if (!string.IsNullOrWhiteSpace(configValidation))
				throw new InvalidOperationException(configValidation);

			var baseUri = _config.HostName.TrimPostfix("/");

			BaseUri = 
				0 < _config.Port
					? $"{baseUri}:{_config.Port}"
					: baseUri;
		}

		protected FtpWebRequest CreateFtpWebRequest(string? path)
		{
			var ftpWebRequest = (FtpWebRequest)WebRequest.Create(UriHelper.Combine(BaseUri, path)!);
			if (!string.IsNullOrWhiteSpace(_config.UserName))
				ftpWebRequest.Credentials = new NetworkCredential(_config.UserName, _config.Password);

			ftpWebRequest.EnableSsl = _config.EnableSsl;
			if (_config.RequestTimeoutInMilliseconds.HasValue)
				ftpWebRequest.Timeout = _config.RequestTimeoutInMilliseconds.Value;

			ftpWebRequest.UseBinary = true;

			return ftpWebRequest;
		}

		public string[] GetDirectoryListing(string relativeDirectoryPath, string searchPattern, bool combineResultsWithPath = true)
		{
			if (string.IsNullOrWhiteSpace(relativeDirectoryPath))
				throw new ArgumentNullException(nameof(relativeDirectoryPath));

			var uri = relativeDirectoryPath;
			if (!string.IsNullOrWhiteSpace(searchPattern))
				uri = UriHelper.Combine(uri, searchPattern);

			var request = CreateFtpWebRequest(uri);
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			using var response = (FtpWebResponse)request.GetResponse();
			using var streamReader = new System.IO.StreamReader(response.GetResponseStream());
			string responseString = streamReader.ReadToEnd();
			string[] results = responseString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

			if (combineResultsWithPath)
				results = results.Select(x => UriHelper.Combine(relativeDirectoryPath, x)).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray()!;

			return results;
		}

		public async Task<string[]> GetDirectoryListingAsync(string relativeDirectoryPath, string searchPattern, bool combineResultsWithPath = true, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(relativeDirectoryPath))
				throw new ArgumentNullException(nameof(relativeDirectoryPath));

			var uri = relativeDirectoryPath;
			if (!string.IsNullOrWhiteSpace(searchPattern))
				uri = UriHelper.Combine(uri, searchPattern);

			var request = CreateFtpWebRequest(uri);
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			using var response = (FtpWebResponse)await request.GetResponseAsync();
			using var streamReader = new System.IO.StreamReader(response.GetResponseStream());
			string responseString = streamReader.ReadToEnd();
			string[] results = responseString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

			if (combineResultsWithPath)
				results = results.Select(x => UriHelper.Combine(relativeDirectoryPath, x)).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray()!;

			return results;
		}

		public System.IO.MemoryStream DownloadFileAsStream(string relativeFilePath)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			var ms = new System.IO.MemoryStream();
			using var response = (FtpWebResponse)request.GetResponse();
			using var responseStream = response.GetResponseStream();
			responseStream.CopyTo(ms);
			return ms;
		}

		public async Task<System.IO.MemoryStream> DownloadFileAsStreamAsync(string relativeFilePath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			var ms = new System.IO.MemoryStream();
			using var response = (FtpWebResponse)await request.GetResponseAsync();
			using var responseStream = response.GetResponseStream();
#if NETSTANDARD2_0 || NETSTANDARD2_1
			await responseStream.CopyToAsync(ms);
#elif NET5_0
			await responseStream.CopyToAsync(ms, cancellationToken);
#endif
			return ms;
		}

		public byte[] DownloadFileAsByteArray(string relativeFilePath)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			var ms = new System.IO.MemoryStream();
			using var response = (FtpWebResponse)request.GetResponse();
			using var responseStream = response.GetResponseStream();
			responseStream.CopyTo(ms);
			return ms.ToArray();
		}

		public async Task<byte[]> DownloadFileAsByteArrayAsync(string relativeFilePath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			var ms = new System.IO.MemoryStream();
			using var response = (FtpWebResponse)await request.GetResponseAsync();
			using var responseStream = response.GetResponseStream();
#if NETSTANDARD2_0 || NETSTANDARD2_1
			await responseStream.CopyToAsync(ms);
#elif NET5_0
			await responseStream.CopyToAsync(ms, cancellationToken);
#endif
			return ms.ToArray();
		}

		public bool DirectoryExists(string relativeDirectoryPath)
		{
			if (string.IsNullOrWhiteSpace(relativeDirectoryPath))
				throw new ArgumentNullException(nameof(relativeDirectoryPath));

			var request = CreateFtpWebRequest(relativeDirectoryPath);
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			try
			{
				using var response = (FtpWebResponse)request.GetResponse();
				return true;
			}
			catch (WebException ex)
			{
				var response = (FtpWebResponse)ex.Response!;
				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
					return false;

				throw;
			}
		}

		public async Task<bool> DirectoryExistsAsync(string relativeDirectoryPath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(relativeDirectoryPath))
				throw new ArgumentNullException(nameof(relativeDirectoryPath));

			var request = CreateFtpWebRequest(relativeDirectoryPath);
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			try
			{
				using var response = (FtpWebResponse)await request.GetResponseAsync();
				return true;
			}
			catch (WebException ex)
			{
				var response = (FtpWebResponse)ex.Response!;
				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
					return false;

				throw;
			}
		}

		public bool FileExists(string relativeFilePath)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

			try
			{
				var response = (FtpWebResponse)request.GetResponse();
				return true;
			}
			catch (WebException ex)
			{
				var response = (FtpWebResponse)ex.Response!;
				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
					return false;

				throw;
			}
		}

		public async Task<bool> FileExistsAsync(string relativeFilePath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

			try
			{
				using var response = (FtpWebResponse)await request.GetResponseAsync();
				return true;
			}
			catch (WebException ex)
			{
				var response = (FtpWebResponse)ex.Response!;
				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
					return false;

				throw;
			}
		}

		public bool CopyFile(string sourceRleativeFilePath, string destRleativeFilePath, bool overwrite)
		{
			if (string.IsNullOrWhiteSpace(sourceRleativeFilePath))
				throw new ArgumentNullException(nameof(sourceRleativeFilePath));

			if (string.IsNullOrWhiteSpace(destRleativeFilePath))
				throw new ArgumentNullException(nameof(destRleativeFilePath));

			if (!overwrite)
			{
				var exists = FileExists(destRleativeFilePath);
				if (exists)
					throw new InvalidOperationException($"File {destRleativeFilePath} already exists.");
			}

			var dirPath = System.IO.Path.GetDirectoryName(destRleativeFilePath);
			CreateDirectory(dirPath!);

			//download
			var getFileRequest = CreateFtpWebRequest(sourceRleativeFilePath);
			getFileRequest.Method = WebRequestMethods.Ftp.DownloadFile;
			using var getFileResponse = (FtpWebResponse)getFileRequest.GetResponse();
			using var responseStream = getFileResponse.GetResponseStream();
			using var ms = new System.IO.MemoryStream();
			responseStream.CopyTo(ms);
			ms.Seek(0, System.IO.SeekOrigin.Begin);

			//upload
			var uploadFileRequest = CreateFtpWebRequest(destRleativeFilePath);
			uploadFileRequest.Method = WebRequestMethods.Ftp.UploadFile;
			uploadFileRequest.ContentLength = ms.Length;
			using (var requestStream = uploadFileRequest.GetRequestStream())
			{
				ms.CopyTo(requestStream);
			}

			using var uploadFileResponse = (FtpWebResponse)uploadFileRequest.GetResponse();
			return true;
				//uploadFileResponse.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(uploadFileResponse.StatusDescription) ? $"Status code: {uploadFileResponse.StatusCode}" : uploadFileResponse.StatusDescription);
		}

		public async Task<bool> CopyFileAsync(string sourceRleativeFilePath, string destRleativeFilePath, bool overwrite, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(sourceRleativeFilePath))
				throw new ArgumentNullException(nameof(sourceRleativeFilePath));

			if (string.IsNullOrWhiteSpace(destRleativeFilePath))
				throw new ArgumentNullException(nameof(destRleativeFilePath));

			if (!overwrite)
			{
				var exists = FileExists(destRleativeFilePath);
				if (exists)
					throw new InvalidOperationException($"File {destRleativeFilePath} already exists.");
			}

			var dirPath = System.IO.Path.GetDirectoryName(destRleativeFilePath);
			await CreateDirectoryAsync(dirPath!, cancellationToken);

			//download
			var getFileRequest = CreateFtpWebRequest(sourceRleativeFilePath);
			getFileRequest.Method = WebRequestMethods.Ftp.DownloadFile;
			using var getFileResponse = (FtpWebResponse)await getFileRequest.GetResponseAsync();
			using var responseStream = getFileResponse.GetResponseStream();
			using var ms = new System.IO.MemoryStream();
#if NETSTANDARD2_0 || NETSTANDARD2_1
			await responseStream.CopyToAsync(ms);
#elif NET5_0
			await responseStream.CopyToAsync(ms, cancellationToken);
#endif
			ms.Seek(0, System.IO.SeekOrigin.Begin);

			//upload
			var uploadFileRequest = CreateFtpWebRequest(destRleativeFilePath);
			uploadFileRequest.Method = WebRequestMethods.Ftp.UploadFile;
			uploadFileRequest.ContentLength = ms.Length;
			using (var requestStream = await uploadFileRequest.GetRequestStreamAsync())
			{
#if NETSTANDARD2_0 || NETSTANDARD2_1
				await ms.CopyToAsync(requestStream);
#elif NET5_0
				await ms.CopyToAsync(requestStream, cancellationToken);
#endif
			}

			using var uploadFileResponse = (FtpWebResponse)await uploadFileRequest.GetResponseAsync();
			return true;
				//uploadFileResponse.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(uploadFileResponse.StatusDescription) ? $"Status code: {uploadFileResponse.StatusCode}" : uploadFileResponse.StatusDescription);
		}

		public bool MoveFile(string sourceRleativeFilePath, string destRleativeFilePath, bool overwrite)
		{
			if (string.IsNullOrWhiteSpace(sourceRleativeFilePath))
				throw new ArgumentNullException(nameof(sourceRleativeFilePath));

			if (string.IsNullOrWhiteSpace(destRleativeFilePath))
				throw new ArgumentNullException(nameof(destRleativeFilePath));

			if (!overwrite)
			{
				var exists = FileExists(destRleativeFilePath);
				if (exists)
					throw new InvalidOperationException($"File {destRleativeFilePath} already exists.");
			}

			var dirPath = System.IO.Path.GetDirectoryName(destRleativeFilePath);
			CreateDirectory(dirPath!);

			//download
			var getFileRequest = CreateFtpWebRequest(sourceRleativeFilePath);
			getFileRequest.Method = WebRequestMethods.Ftp.DownloadFile;
			using var getFileResponse = (FtpWebResponse)getFileRequest.GetResponse();
			using var responseStream = getFileResponse.GetResponseStream();
			using var ms = new System.IO.MemoryStream();
			responseStream.CopyTo(ms);
			ms.Seek(0, System.IO.SeekOrigin.Begin);

			//upload
			var uploadFileRequest = CreateFtpWebRequest(destRleativeFilePath);
			uploadFileRequest.Method = WebRequestMethods.Ftp.UploadFile;
			uploadFileRequest.ContentLength = ms.Length;
			using (var requestStream = uploadFileRequest.GetRequestStream())
			{
				ms.CopyTo(requestStream);
			}

			using var uploadFileResponse = (FtpWebResponse)uploadFileRequest.GetResponse();
			//if (uploadFileResponse.StatusCode != FtpStatusCode.CommandOK)
			//	throw new InvalidOperationException(string.IsNullOrWhiteSpace(uploadFileResponse.StatusDescription) ? $"Status code: {uploadFileResponse.StatusCode}" : uploadFileResponse.StatusDescription);

			//delete
			var deleteRequest = CreateFtpWebRequest(sourceRleativeFilePath);
			deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;

			using var response = (FtpWebResponse)deleteRequest.GetResponse();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public async Task<bool> MoveFileAsync(string sourceRleativeFilePath, string destRleativeFilePath, bool overwrite, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(sourceRleativeFilePath))
				throw new ArgumentNullException(nameof(sourceRleativeFilePath));

			if (string.IsNullOrWhiteSpace(destRleativeFilePath))
				throw new ArgumentNullException(nameof(destRleativeFilePath));

			if (!overwrite)
			{
				var exists = FileExists(destRleativeFilePath);
				if (exists)
					throw new InvalidOperationException($"File {destRleativeFilePath} already exists.");
			}

			var dirPath = System.IO.Path.GetDirectoryName(destRleativeFilePath);
			await CreateDirectoryAsync(dirPath!, cancellationToken);

			//download
			var getFileRequest = CreateFtpWebRequest(sourceRleativeFilePath);
			getFileRequest.Method = WebRequestMethods.Ftp.DownloadFile;
			using var getFileResponse = (FtpWebResponse)await getFileRequest.GetResponseAsync();
			using var responseStream = getFileResponse.GetResponseStream();
			using var ms = new System.IO.MemoryStream();
#if NETSTANDARD2_0 || NETSTANDARD2_1
			await responseStream.CopyToAsync(ms);
#elif NET5_0
			await responseStream.CopyToAsync(ms, cancellationToken);
#endif
			ms.Seek(0, System.IO.SeekOrigin.Begin);

			//upload
			var uploadFileRequest = CreateFtpWebRequest(destRleativeFilePath);
			uploadFileRequest.Method = WebRequestMethods.Ftp.UploadFile;
			uploadFileRequest.ContentLength = ms.Length;
			using (var requestStream = await uploadFileRequest.GetRequestStreamAsync())
			{
#if NETSTANDARD2_0 || NETSTANDARD2_1
				await ms.CopyToAsync(requestStream);
#elif NET5_0
				await ms.CopyToAsync(requestStream, cancellationToken);
#endif
			}

			using var uploadFileResponse = (FtpWebResponse)await uploadFileRequest.GetResponseAsync();
			//if (uploadFileResponse.StatusCode != FtpStatusCode.CommandOK)
			//	throw new InvalidOperationException(string.IsNullOrWhiteSpace(uploadFileResponse.StatusDescription) ? $"Status code: {uploadFileResponse.StatusCode}" : uploadFileResponse.StatusDescription);

			//delete
			var deleteRequest = CreateFtpWebRequest(sourceRleativeFilePath);
			deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;

			using var response = (FtpWebResponse)await deleteRequest.GetResponseAsync();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public bool UploadFile(string relativeFilePath, byte[] sourceBytes)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			if (sourceBytes == null)
				throw new ArgumentNullException(nameof(sourceBytes));

			if (sourceBytes == null)
				throw new ArgumentNullException(nameof(sourceBytes));

			var dirPath = System.IO.Path.GetDirectoryName(relativeFilePath);
			CreateDirectory(dirPath!);

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.UploadFile;

			request.ContentLength = sourceBytes.Length;

			using (var requestStream = request.GetRequestStream())
			{
				requestStream.Write(sourceBytes, 0, sourceBytes.Length);
			}

			using var response = (FtpWebResponse)request.GetResponse();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public async Task<bool> UploadFileAsync(string relativeFilePath, byte[] sourceBytes, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			if (sourceBytes == null)
				throw new ArgumentNullException(nameof(sourceBytes));

			if (sourceBytes == null)
				throw new ArgumentNullException(nameof(sourceBytes));

			var dirPath = System.IO.Path.GetDirectoryName(relativeFilePath);
			await CreateDirectoryAsync(dirPath!, cancellationToken);

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.UploadFile;

			request.ContentLength = sourceBytes.Length;

			using (var requestStream = await request.GetRequestStreamAsync())
			{
				await requestStream.WriteAsync(sourceBytes, 0, sourceBytes.Length, cancellationToken);
			}

			using var response = (FtpWebResponse)await request .GetResponseAsync();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public bool UploadFile(string relativeFilePath, System.IO.Stream sourceStream)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));

			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));

			var dirPath = System.IO.Path.GetDirectoryName(relativeFilePath);
			CreateDirectory(dirPath!);

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.UploadFile;

			var fileContents = sourceStream.ToArray();
			request.ContentLength = fileContents.Length;

			using (var requestStream = request.GetRequestStream())
			{
				requestStream.Write(fileContents, 0, fileContents.Length);
			}

			using var response = (FtpWebResponse)request.GetResponse();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public async Task<bool> UploadFileAsync(string relativeFilePath, System.IO.Stream sourceStream, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));

			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));

			var dirPath = System.IO.Path.GetDirectoryName(relativeFilePath);
			await CreateDirectoryAsync(dirPath!, cancellationToken);

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.UploadFile;

			var fileContents = sourceStream.ToArray();
			request.ContentLength = fileContents.Length;

			using (var requestStream = await request.GetRequestStreamAsync())
			{
				await requestStream.WriteAsync(fileContents, 0, fileContents.Length, cancellationToken);
			}

			using var response = (FtpWebResponse)await request.GetResponseAsync();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		private List<string> SplitDirectoryPath(string direcotryRelativePath)
		{
			direcotryRelativePath = direcotryRelativePath.TrimPrefix("/").TrimPrefix("\\").TrimPostfix("/").TrimPostfix("\\");
			var result = new List<string>();
			var sb = new StringBuilder();
			foreach (var ch in direcotryRelativePath)
			{
				if (ch == '/' || ch == '\\')
				{
					var str = sb.ToString();
					if (!string.IsNullOrWhiteSpace(str))
						result.Add(str);

					sb.Clear();
				}
				else
				{
					sb.Append(ch);
				}
			}

			var lastStr = sb.ToString();
			if (!string.IsNullOrWhiteSpace(lastStr))
				result.Add(lastStr);

			return result;
		}

		public bool CreateDirectory(string direcotryRelativePath)
		{
			if (string.IsNullOrWhiteSpace(direcotryRelativePath))
				throw new ArgumentNullException(nameof(direcotryRelativePath));

			var existsDir = DirectoryExists(direcotryRelativePath);
			if (existsDir)
				return true;

			var sb = new StringBuilder();
			var paths = SplitDirectoryPath(direcotryRelativePath);

			foreach (var chunk in paths)
			{
				var path = sb.Append('/').Append(chunk).ToString();
				existsDir = DirectoryExists(path);
				if (existsDir)
					continue;

				var request = CreateFtpWebRequest(path);
				request.Method = WebRequestMethods.Ftp.MakeDirectory;
				using var response = (FtpWebResponse)request.GetResponse();
			}

			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public async Task<bool> CreateDirectoryAsync(string direcotryRelativePath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(direcotryRelativePath))
				throw new ArgumentNullException(nameof(direcotryRelativePath));

			var existsDir = await DirectoryExistsAsync(direcotryRelativePath, cancellationToken);
			if (existsDir)
				return true;

			var sb = new StringBuilder();
			var paths = SplitDirectoryPath(direcotryRelativePath);

			foreach (var chunk in paths)
			{
				var path = sb.Append('/').Append(chunk).ToString();
				existsDir = await DirectoryExistsAsync(path, cancellationToken);
				if (existsDir)
					continue;

				var request = CreateFtpWebRequest(path);
				request.Method = WebRequestMethods.Ftp.MakeDirectory;
				using var response = (FtpWebResponse)await request.GetResponseAsync();
			}

			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public bool RemoveDirectory(string direcotryRelativePath)
		{
			if (string.IsNullOrWhiteSpace(direcotryRelativePath))
				throw new ArgumentNullException(nameof(direcotryRelativePath));

			var request = CreateFtpWebRequest(direcotryRelativePath);
			request.Method = WebRequestMethods.Ftp.RemoveDirectory;

			using var response = (FtpWebResponse)request.GetResponse();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public async Task<bool> RemoveDirectoryAsync(string direcotryRelativePath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(direcotryRelativePath))
				throw new ArgumentNullException(nameof(direcotryRelativePath));

			var request = CreateFtpWebRequest(direcotryRelativePath);
			request.Method = WebRequestMethods.Ftp.RemoveDirectory;

			using var response = (FtpWebResponse)await request.GetResponseAsync();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public bool DeleteFile(string relativeFilePath)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.DeleteFile;

			using var response = (FtpWebResponse)request.GetResponse();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}

		public async Task<bool> DeleteFileAsync(string relativeFilePath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
				throw new ArgumentNullException(nameof(relativeFilePath));

			var request = CreateFtpWebRequest(relativeFilePath);
			request.Method = WebRequestMethods.Ftp.DeleteFile;

			using var response = (FtpWebResponse)await request.GetResponseAsync();
			return true;
				//response.StatusCode == FtpStatusCode.CommandOK
				//	? true
				//	: throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.StatusDescription) ? $"Status code: {response.StatusCode}" : response.StatusDescription);
		}
	}
}
