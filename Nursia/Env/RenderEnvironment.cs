using AssetManagementBase;
using Microsoft.Xna.Framework;
using Nursia.Env.Sky;
using Nursia.Serialization;

namespace Nursia.Env
{
	/// <summary>
	/// Defines the rendering environment including lighting, fog, and skybox.
	/// </summary>
	public class RenderEnvironment: IHasExternalAssets
	{
		/// <summary>
		/// Gets the default render environment.
		/// </summary>
		public static readonly RenderEnvironment Default = new RenderEnvironment();

		/// <summary>
		/// Gets or sets the ambient light color for the scene.
		/// </summary>
		public Color AmbientLightColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the color of fog in the scene.
		/// </summary>
		public Color FogColor { get; set; } = Color.LightGray;

		/// <summary>
		/// Gets or sets the distance at which fog begins.
		/// </summary>
		public float FogStart { get; set; } = 100.0f;

		/// <summary>
		/// Gets or sets the distance at which fog is fully opaque.
		/// </summary>
		public float FogEnd { get; set; } = 1000.0f;

		/// <summary>
		/// Gets or sets a value indicating whether fog is enabled.
		/// </summary>
		public bool FogEnabled { get; set; } = false;

		/// <summary>
		/// Gets or sets the skybox for this environment.
		/// </summary>
		public Skybox Sky { get; set; }

		/// <summary>
		/// Creates a copy of this render environment.
		/// </summary>
		/// <returns>A cloned render environment with identical properties.</returns>
		public RenderEnvironment Clone()
		{
			return new RenderEnvironment
			{
				AmbientLightColor = AmbientLightColor,
				FogColor = FogColor,
				FogStart = FogStart,
				FogEnd = FogEnd,
				FogEnabled = FogEnabled,
				Sky = Sky?.Clone()
			};
		}

		/// <summary>
		/// Loads all external assets referenced by this environment.
		/// </summary>
		/// <param name="assetManager">The asset manager to load resources from.</param>
		public void Load(AssetManager assetManager)
		{
			Sky?.Load(assetManager);
		}
	}
}
