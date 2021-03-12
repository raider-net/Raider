using Raider.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Authentication
{
	public interface IAuthenticationManager
	{
		bool LogRequestAuthentication { get; }
		bool LogRoles { get; }
		bool LogPermissions { get; }
		int? StaticUserId { get; }

		Task<AuthenticatedUser?> CreateFromWindowsIdentityAsync(string? logonWithoutDomain, string? windowsIdentityName);

		Task<AuthenticatedUser?> CreateFromLoginPasswordAsync(string? login, string? password);

		Task<AuthenticatedUser?> CreateFromLoginAsync(string? login);

		Task<AuthenticatedUser?> CreateFromUserIdAsync(int? idUser);

		Task<AuthenticatedUser?> CreateFromRequestAsync(IDictionary<string, string[]> requestHeaders);

		Task<AuthenticatedUser?> SetRolesAndPremissions(AuthenticatedUser user);

		Task<AuthenticatedUser?> SetUserDataAsync(AuthenticatedUser user);
	}
}
