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

		Task<AuthenticatedUser?> CreateFromWindowsIdentityAsync(string? logonWithoutDomain, string? windowsIdentityName, IRequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> CreateFromLoginPasswordAsync(string? login, string? password);

		Task<AuthenticatedUser?> CreateFromLoginAsync(string? login, IRequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> CreateFromUserIdAsync(int? idUser, IRequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> CreateFromRequestAsync(IRequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> SetUserDataRolesPremissions(AuthenticatedUser user, IRequestMetadata? requestMetadata = null);

		Task<AuthenticatedUser?> SetUserDataAsync(AuthenticatedUser user, IRequestMetadata? requestMetadata = null, List<int>? roleIds = null);
	}
}
