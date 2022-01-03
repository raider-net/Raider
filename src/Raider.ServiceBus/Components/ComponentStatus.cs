namespace Raider.ServiceBus.Components
{
	public enum ComponentStatus
	{
		Unchanged = 0,

		/// <summary>
		/// The component is offline
		/// </summary>
		Offline = 1,

		/// <summary>
		/// The component is waiting for a message
		/// </summary>
		Idle = 2,

		/// <summary>
		/// Tne component is currently in process
		/// </summary>
		InProcess = 3,

		/// <summary>
		/// The component is in error state
		/// </summary>
		Error = 4,

		/// <summary>
		/// The component was suspended
		/// </summary>
		Suspended = 5
	}
}
