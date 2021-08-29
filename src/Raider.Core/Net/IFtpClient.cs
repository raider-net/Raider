using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Net
{
	public interface IFtpClient
	{
		string[] GetDirectoryListing(string relativeDirectoryPath, string searchPattern, bool combineResultsWithPath = true);
		MemoryStream DownloadFileAsStream(string relativeFilePath);
		byte[] DownloadFileAsByteArray(string relativeFilePath);
		bool DirectoryExists(string relativeDirectoryPath);
		bool FileExists(string relativeFilePath);
		bool CopyFile(string sourceRleativeFilePath, string destRleativeFilePath, bool overwrite);
		bool MoveFile(string sourceRleativeFilePath, string destRleativeFilePath, bool overwrite);
		bool UploadFile(string relativeFilePath, byte[] sourceBytes);
		bool UploadFile(string relativeFilePath, Stream sourceStream);
		bool CreateDirectory(string direcotryRelativePath);
		bool RemoveDirectory(string direcotryRelativePath);
		bool DeleteFile(string relativeFilePath);
	}

	public interface IAsyncFtpClient : IFtpClient
	{
		Task<string[]> GetDirectoryListingAsync(string relativeDirectoryPath, string searchPattern, bool combineResultsWithPath = true, CancellationToken cancellationToken = default);
		Task<MemoryStream> DownloadFileAsStreamAsync(string relativeFilePath, CancellationToken cancellationToken = default);
		Task<byte[]> DownloadFileAsByteArrayAsync(string relativeFilePath, CancellationToken cancellationToken = default);
		Task<bool> DirectoryExistsAsync(string relativeDirectoryPath, CancellationToken cancellationToken = default);
		Task<bool> FileExistsAsync(string relativeFilePath, CancellationToken cancellationToken = default);
		Task<bool> CopyFileAsync(string sourceRleativeFilePath, string destRleativeFilePath, bool overwrite, CancellationToken cancellationToken = default);
		Task<bool> MoveFileAsync(string sourceRleativeFilePath, string destRleativeFilePath, bool overwrite, CancellationToken cancellationToken = default);
		Task<bool> UploadFileAsync(string relativeFilePath, byte[] sourceBytes, CancellationToken cancellationToken = default);
		Task<bool> UploadFileAsync(string relativeFilePath, Stream sourceStream, CancellationToken cancellationToken = default);
		Task<bool> CreateDirectoryAsync(string direcotryRelativePath, CancellationToken cancellationToken = default);
		Task<bool> RemoveDirectoryAsync(string direcotryRelativePath, CancellationToken cancellationToken = default);
		Task<bool> DeleteFileAsync(string relativeFilePath, CancellationToken cancellationToken = default);
	}
}
