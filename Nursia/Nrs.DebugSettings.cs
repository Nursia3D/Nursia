namespace Nursia
{
	/// <summary>
	/// Specifies which buffer to visualize for debugging purposes.
	/// </summary>
	public enum DebugVisualizeBuffer
	{
		/// <summary>No visualization.</summary>
		None,

		/// <summary>Visualize the depth buffer.</summary>
		DepthBuffer,

		/// <summary>Visualize the shadow map.</summary>
		ShadowMap,

		/// <summary>Visualize the reflection shadow map.</summary>
		ReflectionShadowMap,

		/// <summary>Visualize the reflection map.</summary>
		ReflectionMap
	}

	partial class Nrs
	{
		/// <summary>
		/// Provides debug settings for the Nursia engine.
		/// </summary>
		public class DebugSettingsType
		{
			/// <summary>
			/// Gets or sets a value indicating whether to draw bounding boxes.
			/// </summary>
			public bool DrawBoundingBoxes { get; set; }

			/// <summary>
			/// Gets or sets a value indicating whether to disable normal mapping.
			/// </summary>
			public bool DisableNormalMap { get; set; }

			/// <summary>
			/// Gets or sets which buffer to visualize for debugging.
			/// </summary>
			public DebugVisualizeBuffer VisualizeBuffer { get; set; }
		}

		private static DebugSettingsType _debugSettingsInstance = new DebugSettingsType();

		/// <summary>
		/// Gets or sets the current debug settings for the engine.
		/// </summary>
		public static DebugSettingsType DebugSettings
		{
			get => _debugSettingsInstance;

			set
			{
				_debugSettingsInstance = value;
			}
		}
	}
}
