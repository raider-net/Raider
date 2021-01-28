using System;
using System.Text;

namespace Raider.Identity
{
	public class BasicAuthentication
	{
		public string? UserName { get; set; }

		public string? Password { get; set; }

		public BasicAuthentication()
		{
		}

		public BasicAuthentication(string? userName, string? password)
		{
			UserName = userName;
			Password = password;
		}

		public override string ToString()
		{
			var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{UserName}:{Password}"));
			return $"Basic {token}";
		}
	}
}
