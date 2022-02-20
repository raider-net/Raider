using Raider.Trace;
using System;
using System.Collections.Generic;

namespace Raider.Identity
{
	public class AuthenticatedUser
	{
		public int UserId { get; }
		public string Login { get; }
		public string DisplayName { get; }
		public object? UserData { get; set; }
		public List<string>? Roles { get; set; }
		public List<string>? Permissions { get; set; }
		public List<int>? RoleIds { get; set; }
		public List<int>? PermissionIds { get; set; }

		public ITraceInfo TraceInfo { get; }

		public string? Password { get; set; }
		public string? Salt { get; set; }
		public string? Error { get; set; }
		public string? PasswordTemporaryUrlSlug { get; set; }

		public AuthenticatedUser(int userId, string login, string? displayName, ITraceInfo traceInfo)
		{
			UserId = userId;
			Login = string.IsNullOrWhiteSpace(login)
				? throw new ArgumentNullException(nameof(login))
				: login;
			DisplayName = string.IsNullOrWhiteSpace(displayName)
				? Login
				: displayName;
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
		}
	}

	public class AnonymousUser : AuthenticatedUser
	{
		public const string AnonymousRaiderUserName = "AnonymousRaiderUser";
		public const int AnonymousRaiderUserId = -1;

		public AnonymousUser(ITraceInfo traceInfo)
			: base(AnonymousRaiderUserId, AnonymousRaiderUserName, null, traceInfo)
		{
		}
	}
}
