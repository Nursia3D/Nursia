using Microsoft.Xna.Framework.Graphics;
using Nursia.Env;
using Nursia.Rendering;
using Nursia.SceneGraph.Lights;
using System;

namespace Nursia.SceneGraph
{
	partial class Scene : IRenderSource
	{
		private static Action<SceneNode, Camera, ILightsBatch> _lightHandler = new Action<SceneNode, Camera, ILightsBatch>((n, c, b) =>
		{
			var asLight = n as BaseLight;

			if (asLight != null)
			{
				b.AddLight(asLight);
			}
		});

		private static Action<SceneNode, Camera, IRenderJobsBatch> _renderHandler = new Action<SceneNode, Camera, IRenderJobsBatch>((n, c, b) =>
		{
			n.AddRenderJobs(c, b);
		});

		void IRenderSource.QueryLights(Camera camera, ILightsBatch batch)
		{
			Root.Traverse(_lightHandler, camera, batch);
		}

		void IRenderSource.QueryRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			Root.Traverse(_renderHandler, camera, batch);
		}

		public void Render(ForwardRenderer renderer, Camera camera, RenderEnvironment renderEnvironment)
		{
			renderer.Render(this, camera, renderEnvironment);
		}

		public void Render(ForwardRenderer renderer, Camera camera) => Render(renderer, camera, RenderEnvironment);

		public void Render(ForwardRenderer renderer) => Render(renderer, Camera, RenderEnvironment);

		public RenderTarget2D RenderToTarget(ForwardRenderer renderer, Camera camera, RenderEnvironment renderEnvironment, int width, int height)
		{
			return renderer.RenderToTarget(this, camera, renderEnvironment, width, height);
		}

		public RenderTarget2D RenderToTarget(ForwardRenderer renderer, Camera camera, int width, int height) => RenderToTarget(renderer, camera, RenderEnvironment, width, height);

		public RenderTarget2D RenderToTarget(ForwardRenderer renderer, int width, int height) => RenderToTarget(renderer, Camera, RenderEnvironment, width, height);
	}
}
