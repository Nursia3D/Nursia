using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.SceneGraph;

namespace Nursia.Shadows
{
	internal class DirectLightCSMData
	{
		public const int MaxShadowCascadesCount = 4;

		public readonly Camera[] SourceCameras = new Camera[MaxShadowCascadesCount];
		public readonly Matrix[] LightViewProjs = new Matrix[MaxShadowCascadesCount];
		public readonly Camera[] Cameras = new Camera[MaxShadowCascadesCount];

		public int CascadesCount { get; set; }
		public RenderTarget2D ShadowMap { get; set; }

		public DirectLightCSMData()
		{
			for (var i = 0; i < Cameras.Length; ++i)
			{
				SourceCameras[i] = new Camera();
				Cameras[i] = new Camera();
			}
		}
	}
}
