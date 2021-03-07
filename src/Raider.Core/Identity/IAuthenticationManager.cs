using Raider.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Authentication
{
	public interface IAuthenticationManager
	{
		int? StaticUserId { get; }

		Task<UserRoleActivities?> CreateFromWindowsIdentityAsync(string? logonWithoutDomain, string? windowsIdentityName);

		Task<UserRoleActivities?> CreateFromLoginPasswordAsync(string? login, string? password);

		Task<UserRoleActivities?> CreateFromLoginAsync(string? login);

		Task<UserRoleActivities?> CreateFromUserIdAsync(int? idUser);

		Task<UserRoleActivities?> CreateFromRequestAsync(IDictionary<string, string[]> requestHeaders);

		Task<UserRoleActivities?> SetRolesAndActivities(UserRoleActivities user);

		Task<UserRoleActivities?> SetUserDataAsync(UserRoleActivities user);
	}
}
