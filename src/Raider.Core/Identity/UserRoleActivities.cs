using System;
using System.Collections.Generic;

namespace Raider.Identity
{
	public class UserRoleActivities
	{
		public int UserId { get; set; }
		public string Login { get; set; }
		public string DisplayName { get; set; }
		public object? UserData { get; set; }
		public List<string>? Roles { get; set; }
		public List<int>? RoleIds { get; set; }
		public List<string>? Activities { get; set; }

		public string? Password { get; set; }
		public string? Salt { get; set; }
		public string? Error { get; set; }
		public string? PasswordTemporaryUrlSlug { get; set; }

		public UserRoleActivities(int userId, string login, string? displayName)
		{
			UserId = userId;
			Login = string.IsNullOrWhiteSpace(login)
				? throw new ArgumentNullException(nameof(login))
				: login;
			DisplayName = string.IsNullOrWhiteSpace(displayName)
				? Login
				: displayName;
		}
	}
}
