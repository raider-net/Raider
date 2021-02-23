namespace Raider.Logging
{
	public enum LogCode // Max length 30
	{
		/// <summary>
		/// ApplicationStart
		/// </summary>
		App_Start,

		/// <summary>
		/// MethodEntry
		/// </summary>
		Method_In,

		/// <summary>
		/// MethodExit
		/// </summary>
		Method_Out,

		/// <summary>
		/// EnvironmentInfo
		/// </summary>
		Env_Info,



		//*******************************//
		//*********EXCEPTIONS**********//
		//*******************************//


		/// <summary>
		/// ArgumentException
		/// </summary>
		Ex_Arg,

		/// <summary>
		/// ArgumentNullException
		/// </summary>
		Ex_ArgNull,

		/// <summary>
		/// ArgumentOutOfRangeException
		/// </summary>
		Ex_ArgRange,

		/// <summary>
		/// InvalidOperationException
		/// </summary>
		Ex_InvOp,

		/// <summary>
		/// NotImplementedException
		/// </summary>
		Ex_NotImpl,

		/// <summary>
		/// NotSupportedException
		/// </summary>
		Ex_NotSupp,

		/// <summary>
		/// ApplicationException
		/// </summary>
		Ex_App
	}
}
