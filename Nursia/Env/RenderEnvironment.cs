using AssetManagementBase;
using Microsoft.Xna.Framework;
using Nursia.Env.Sky;
using Nursia.Serialization;

namespace Nursia.Env
{
	public class RenderEnvironment: IHasExternalAssets
	{
		public static readonly RenderEnvironment Default = new RenderEnvironment();
		
		public Color AmbientLightColor { get; set; } = Color.Black;

		public Color FogColor { get; set; } = Color.LightGray;

		public float FogStart { get; set; } = 100.0f;
		public float FogEnd { get; set; } = 1000.0f;
		public bool FogEnabled { get; set; } = false;
		public Skybox Sky { get; set; }

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

		public void Load(AssetManager assetManager)
		{
			Sky?.Load(assetManager);
		}
	}
}
