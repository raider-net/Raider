//using Raider.Identity;
//using System;
//using System.Collections.Generic;

//namespace Raider.AspNetCore.Authentication
//{
//	public class StaticAuthenticationManager : IAuthenticationManager
//	{
//		public int? StaticUserId => StaticAuthenticationManagerSettings.StaticUserId;

//		public UserRoleActivities? CreateFromWindowsIdentityAsync(string? logonWithoutDomain, string? windowsIdentityName)
//			=> StaticAuthenticationManagerSettings.CreateFromWindowsIdentity == null
//				? throw new NotSupportedException(nameof(CreateFromWindowsIdentityAsync))
//				: StaticAuthenticationManagerSettings.CreateFromWindowsIdentity?.Invoke(logonWithoutDomain, windowsIdentityName);

//		public UserRoleActivities? CreateFromRequest(IDictionary<string, string[]> requestHeadersDictionary)
//			=> StaticAuthenticationManagerSettings.CreateFromRequest == null
//				? throw new NotSupportedException(nameof(CreateFromRequest))
//				: StaticAuthenticationManagerSettings.CreateFromRequest?.Invoke(requestHeadersDictionary);

//		public UserRoleActivities? CreateFromLoginPassword(string? login, string? password)
//			=> StaticAuthenticationManagerSettings.CreateFromLoginPassword == null
//				? throw new NotSupportedException(nameof(CreateFromLoginPassword))
//				: StaticAuthenticationManagerSettings.CreateFromLoginPassword?.Invoke(login, password);

//		public UserRoleActivities? CreateFromLogin(string? login)
//			=> StaticAuthenticationManagerSettings.CreateFromLogin == null
//				? throw new NotSupportedException(nameof(CreateFromLogin))
//				: StaticAuthenticationManagerSettings.CreateFromLogin?.Invoke(login);

//		public UserRoleActivities? CreateFromUserId(int? idUser)
//			=> StaticAuthenticationManagerSettings.CreateFromUserId == null
//				? throw new NotSupportedException(nameof(CreateFromUserId))
//				: StaticAuthenticationManagerSettings.CreateFromUserId?.Invoke(idUser);

//		public UserRoleActivities? SetUserData(UserRoleActivities userRoleActivities)
//			=> StaticAuthenticationManagerSettings.SetUserData == null
//				? throw new NotSupportedException(nameof(SetUserData))
//				: StaticAuthenticationManagerSettings.SetUserData?.Invoke(userRoleActivities);
//	}

//	public static class StaticAuthenticationManagerSettings
//	{
//		public static int? StaticUserId { get; set; }

//		public static Func<string?, string?, UserRoleActivities?>? CreateFromWindowsIdentity { get; set; }

//		public static Func<IDictionary<string, string[]>, UserRoleActivities?>? CreateFromRequest { get; set; }

//		public static Func<string?, string?, UserRoleActivities?>? CreateFromLoginPassword { get; set; }

//		public static Func<string?, UserRoleActivities?>? CreateFromLogin { get; set; }

//		public static Func<int?, UserRoleActivities?>? CreateFromUserId { get; set; }

//		public static Func<UserRoleActivities, UserRoleActivities?>? SetUserData { get; set; }
//	}
//}
