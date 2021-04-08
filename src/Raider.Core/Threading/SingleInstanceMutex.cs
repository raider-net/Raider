using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Raider.Threading
{
	public static class SingleInstanceMutex
	{
		private static readonly object _lock = new();

		private static Mutex? _mutex;
		private static bool _started = false;

		/// <summary>
		/// Can be started only once per runtime
		/// </summary>
		public static bool Start(string instanceIdentifier)
		{
			if (_mutex != null || _started)
				throw new ApplicationException("Multiple instance start");

			lock (_lock)
			{
				if (_mutex != null || _started)
					throw new ApplicationException("Multiple instance start");

				_mutex = new Mutex(true, instanceIdentifier, out _started);
				return _started;
			}
		}

		public static void Stop()
		{
			if (_mutex == null || !_started)
				return;

			lock (_lock)
			{
				if (_mutex == null || !_started)
					return;

				_mutex.ReleaseMutex();
				_mutex = null;
			}
		}

		public static bool KillOtherProcessesByName(
			Func<List<Process>, List<Process>> onOtherProcessesFound,
			Action<List<Process>> onNotKilledProcesses)
		{
			var currentProcess = Process.GetCurrentProcess();
			var listOfProcs = Process.GetProcessesByName(currentProcess.ProcessName)?.Where(p => p.Id != currentProcess.Id).ToList();

			if (listOfProcs != null && 0 < listOfProcs.Count)
			{
				listOfProcs = onOtherProcessesFound?.Invoke(listOfProcs);

				if (listOfProcs != null && 0 < listOfProcs.Count)
				{
					var notKilledProcesses = new List<Process>();

					foreach (Process proc in listOfProcs)
					{
						try
						{
							proc.Kill();
						}
						catch
						{
							notKilledProcesses.Add(proc);
						}
					}

					onNotKilledProcesses?.Invoke(notKilledProcesses);

					return notKilledProcesses.Count == 0;
				}
			}

			return true;
		}
	}
}
