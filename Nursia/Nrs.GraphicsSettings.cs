using Nursia.Shadows;
using System;
using System.ComponentModel;

namespace Nursia
{
	/// <summary>
	/// Specifies the type of shadow rendering to use.
	/// </summary>
	public enum ShadowType
	{
		/// <summary>No shadows.</summary>
		None,

		/// <summary>Simple shadow rendering.</summary>
		Simple,

		/// <summary>Percentage-Closer Filtering shadows.</summary>
		PCF
	}

	partial class Nrs
	{
		/// <summary>
		/// Provides graphics rendering settings for the Nursia engine.
		/// </summary>
		public class GraphicsSettingsType
		{
			private readonly ShadowCascadeManager _cascadeManager = new ShadowCascadeManager();

			/// <summary>
			/// Gets or sets the shadow rendering technique to use.
			/// </summary>
			[Category("Shadows")]
			[DefaultValue(ShadowType.Simple)]
			public ShadowType ShadowType { get; set; } = ShadowType.Simple;

			/// <summary>
			/// Gets or sets the size of shadow maps.
			/// </summary>
			[Category("Shadows")]
			public ShadowMapSize ShadowMapSize { get; set; } = ShadowMapSize.Size4096;

			/// <summary>
			/// Gets or sets the shadow base value that controls minimum shadow intensity.
			/// </summary>
			[Category("Shadows")]
			public float ShadowBase { get; set; } = 0.0f;

			/// <summary>
			/// Gets or sets the shadow intensity multiplier.
			/// </summary>
			[Category("Shadows")]
			public float ShadowIntensity { get; set; } = 0.6f;

			/// <summary>
			/// Gets or sets the bias value to reduce shadow acne artifacts.
			/// </summary>
			[Category("Shadows")]
			public float ShadowBias { get; set; } = 0.0f;

			/// <summary>
			/// Gets or sets the distance where shadows start fading.
			/// </summary>
			[Category("Shadows")]
			public float ShadowFadeStart { get; set; } = 0.8f;

			/// <summary>
			/// Gets or sets the number of shadow cascades for directional shadows.
			/// </summary>
			[Category("Shadows")]
			public int Cascades
			{
				get => _cascadeManager.Cascades;

				set => _cascadeManager.Cascades = value;
			}

			/// <summary>
			/// Gets or sets the maximum distance for shadow casting.
			/// </summary>
			[Category("Shadows")]
			public float MaxShadowDistance
			{
				get => _cascadeManager.MaxDistance;

				set => _cascadeManager.MaxDistance = value;
			}

			/// <summary>
			/// Gets the shadow cascade manager.
			/// </summary>
			internal ShadowCascadeManager ShadowCascadeManager => _cascadeManager;

			/// <summary>
			/// Initializes a new instance of the <see cref="GraphicsSettingsType"/> class.
			/// </summary>
			internal GraphicsSettingsType()
			{
			}
		}

		private static GraphicsSettingsType _graphicsSettingsInstance = new GraphicsSettingsType();

		/// <summary>
		/// Gets or sets the current graphics settings for the engine.
		/// </summary>
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
