using Raider.Identity;
using Raider.Web;
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

		Task<AuthenticatedUser?> CreateFromWindowsIdentityAsync(string? logonWithoutDomain, string? windowsIdentityName, RequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> CreateFromLoginPasswordAsync(string? login, string? password);

		Task<AuthenticatedUser?> CreateFromLoginAsync(string? login, RequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> CreateFromUserIdAsync(int? idUser, RequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> CreateFromRequestAsync(RequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> SetUserDataRolesPremissions(AuthenticatedUser user, RequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> SetUserDataAsync(AuthenticatedUser user, RequestMetadata? requestMetadata = null);
	}
}
