namespace DigitalRiseModel.Samples.Character
{
	/// <summary>
	/// Static configuration class for the Character sample application.
	/// Stores global settings that affect game behavior.
	/// </summary>
	static class Configuration
	{
		/// <summary>
		/// Gets or sets whether to disable fixed timestep updates.
		/// When false (default), updates run at a fixed rate.
		/// When true, updates run as fast as possible.
		/// </summary>
		public static bool NoFixedStep { get; set; }
	}
}
