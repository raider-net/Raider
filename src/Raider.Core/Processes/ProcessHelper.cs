using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Raider.Processes
{
	public static class ProcessHelper
	{
		public static string? RunExeCommand(
			string executablePath,
			string? arguments,
			out bool errorOccured,
			bool hidden = true)
		{
			if (string.IsNullOrWhiteSpace(executablePath))
			{
				errorOccured = true;
				return $"{nameof(executablePath)} == null";
			}

			string? msg = null;
			errorOccured = false;
			try
			{
				var startInfo = new ProcessStartInfo
				{
					UseShellExecute = false,
					FileName = executablePath,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				};

				var dir = Path.GetDirectoryName(executablePath);

				if (!string.IsNullOrWhiteSpace(dir))
					startInfo.WorkingDirectory = dir;

				if (!string.IsNullOrWhiteSpace(arguments))
					startInfo.Arguments = arguments;

				if (hidden)
				{
					startInfo.CreateNoWindow = true;
					startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				}
				else
				{
					startInfo.CreateNoWindow = false;
					startInfo.WindowStyle = ProcessWindowStyle.Normal;
				}

				using var process = Process.Start(startInfo);
				if (process == null)
				{
					errorOccured = true;
					return $"{nameof(process)} == null";
				}

				var sb = new StringBuilder();
				process.WaitForExit();
				var num = process.ExitCode;
				string? outMsg = process.StandardOutput.ReadToEnd()?.Replace("\0", "");
				string? errorMsg = process.StandardError.ReadToEnd()?.Replace("\0", "");

				if (!string.IsNullOrEmpty(errorMsg))
					sb.AppendLine(errorMsg);
				
				if (!string.IsNullOrEmpty(outMsg))
					sb.AppendLine(outMsg);

				errorOccured = num != 0;
				msg = sb.ToString();
				if (string.IsNullOrWhiteSpace(msg))
					msg = null;
			}
			catch (Exception ex)
			{
				errorOccured = true;
				msg = ex.ToString();
			}

			return msg;
		}

		public static string? RunMsiCommand(
			string msiFilePath,
			string? arguments,
			out bool errorOccured,
			bool hidden = true)
		{
			if (string.IsNullOrWhiteSpace(msiFilePath))
			{
				errorOccured = true;
				return $"{nameof(msiFilePath)} == null";
			}

			string? msg = null;
			errorOccured = false;

			try
			{
				var startInfo = new ProcessStartInfo
				{
					UseShellExecute = false,
					FileName = "msiexec.exe",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					Arguments = $"{(hidden ? "/qn " : "")}/i \"{msiFilePath}{(string.IsNullOrWhiteSpace(arguments) ? "" : $" {arguments}")}\" ALLUSERS=1",
				};

				var dir = Path.GetDirectoryName(msiFilePath);

				if (!string.IsNullOrWhiteSpace(dir))
					startInfo.WorkingDirectory = dir;

				if (hidden)
				{
					startInfo.CreateNoWindow = true;
					startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				}
				else
				{
					startInfo.CreateNoWindow = false;
					startInfo.WindowStyle = ProcessWindowStyle.Normal;
				}

				using var process = Process.Start(startInfo);
				if (process == null)
				{
					errorOccured = true;
					return $"{nameof(process)} == null";
				}

				var sb = new StringBuilder();
				process.WaitForExit();
				var num = process.ExitCode;
				string? outMsg = process.StandardOutput.ReadToEnd()?.Replace("\0", "");
				string? errorMsg = process.StandardError.ReadToEnd()?.Replace("\0", "");

				if (!string.IsNullOrEmpty(errorMsg))
					sb.AppendLine(errorMsg);

				if (!string.IsNullOrEmpty(outMsg))
					sb.AppendLine(outMsg);

				errorOccured = num != 0;
				msg = sb.ToString();
				if (string.IsNullOrWhiteSpace(msg))
					msg = null;
			}
			catch (Exception ex)
			{
				errorOccured = true;
				msg = ex.ToString();
			}

			return msg;
		}
	}
}
