using Nursia.Shadows;
using System;
using System.ComponentModel;

namespace Nursia
{
	public enum ShadowType
	{
		None,
		Simple,
		PCF
	}

	partial class Nrs
	{
		public class GraphicsSettingsType
		{
			private readonly ShadowCascadeManager _cascadeManager = new ShadowCascadeManager();

			[Category("Shadows")]
			[DefaultValue(ShadowType.Simple)]
			public ShadowType ShadowType { get; set; } = ShadowType.Simple;

			[Category("Shadows")]
			public ShadowMapSize ShadowMapSize { get; set; } = ShadowMapSize.Size4096;

			[Category("Shadows")]
			public float ShadowBase { get; set; } = 0.0f;

			[Category("Shadows")]
			public float ShadowIntensity { get; set; } = 0.6f;

			[Category("Shadows")]
			public float ShadowBias { get; set; } = 0.0f;

			[Category("Shadows")]
			public float ShadowFadeStart { get; set; } = 0.8f;

			[Category("Shadows")]
			public int Cascades
			{
				get => _cascadeManager.Cascades;

				set => _cascadeManager.Cascades = value;
			}

			[Category("Shadows")]
			public float MaxShadowDistance
			{
				get => _cascadeManager.MaxDistance;

				set => _cascadeManager.MaxDistance = value;
			}

			internal ShadowCascadeManager ShadowCascadeManager => _cascadeManager;

			internal GraphicsSettingsType()
			{
			}
		}

		private static GraphicsSettingsType _graphicsSettingsInstance = new GraphicsSettingsType();

		public static GraphicsSettingsType GraphicsSettings
		{
			get => _graphicsSettingsInstance;

			set
			{
				_graphicsSettingsInstance = value ?? throw new ArgumentNullException(nameof(value));
			}
		}
	}
}
