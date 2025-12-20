namespace Nursia
{
	public enum DebugVisualizeBuffer
	{
		None,
		DepthBuffer,
		ShadowMap,
		ReflectionShadowMap,
		ReflectionMap
	}

	partial class Nrs
	{
		public class DebugSettingsType
		{
			public bool DrawBoundingBoxes { get; set; }
			public bool DisableNormalMap { get; set; }
			public DebugVisualizeBuffer VisualizeBuffer { get; set; }
		}

		private static DebugSettingsType _debugSettingsInstance = new DebugSettingsType();

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
