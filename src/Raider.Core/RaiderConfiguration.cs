using System;
using System.Text;

namespace Raider
{
	public static class RaiderConfiguration
	{
		public static Action<StringBuilder, Exception>? SerializeFaultException { get; set; }
	}
}
