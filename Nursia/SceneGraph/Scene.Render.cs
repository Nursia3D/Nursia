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

		/// <summary>
		/// Renders this scene using the specified renderer, camera, and render environment.
		/// </summary>
		/// <param name="renderer">The forward renderer to use for rendering.</param>
		/// <param name="camera">The camera to render from.</param>
		/// <param name="renderEnvironment">The render environment defining lighting and atmosphere.</param>
		public void Render(ForwardRenderer renderer, Camera camera, RenderEnvironment renderEnvironment)
		{
			renderer.Render(this, camera, renderEnvironment);
		}

		/// <summary>
		/// Renders this scene using the specified renderer and camera, with this scene's render environment.
		/// </summary>
		/// <param name="renderer">The forward renderer to use for rendering.</param>
		/// <param name="camera">The camera to render from.</param>
		public void Render(ForwardRenderer renderer, Camera camera) => Render(renderer, camera, RenderEnvironment);

		/// <summary>
		/// Renders this scene using the specified renderer, with this scene's camera and render environment.
		/// </summary>
		/// <param name="renderer">The forward renderer to use for rendering.</param>
		public void Render(ForwardRenderer renderer) => Render(renderer, Camera, RenderEnvironment);

		/// <summary>
		/// Renders this scene to a render target with the specified dimensions.
		/// </summary>
		/// <param name="renderer">The forward renderer to use for rendering.</param>
		/// <param name="camera">The camera to render from.</param>
		/// <param name="renderEnvironment">The render environment defining lighting and atmosphere.</param>
		/// <param name="width">The width of the render target.</param>
		/// <param name="height">The height of the render target.</param>
		/// <returns>A render target containing the rendered scene.</returns>
		public RenderTarget2D RenderToTarget(ForwardRenderer renderer, Camera camera, RenderEnvironment renderEnvironment, int width, int height)
		{
			return renderer.RenderToTarget(this, camera, renderEnvironment, width, height);
		}

		/// <summary>
		/// Renders this scene to a render target using this scene's render environment.
		/// </summary>
		/// <param name="renderer">The forward renderer to use for rendering.</param>
		/// <param name="camera">The camera to render from.</param>
		/// <param name="width">The width of the render target.</param>
		/// <param name="height">The height of the render target.</param>
		/// <returns>A render target containing the rendered scene.</returns>
		public RenderTarget2D RenderToTarget(ForwardRenderer renderer, Camera camera, int width, int height) => RenderToTarget(renderer, camera, RenderEnvironment, width, height);

		/// <summary>
		/// Renders this scene to a render target using this scene's camera and render environment.
		/// </summary>
		/// <param name="renderer">The forward renderer to use for rendering.</param>
		/// <param name="width">The width of the render target.</param>
		/// <param name="height">The height of the render target.</param>
		/// <returns>A render target containing the rendered scene.</returns>
		public RenderTarget2D RenderToTarget(ForwardRenderer renderer, int width, int height) => RenderToTarget(renderer, Camera, RenderEnvironment, width, height);
	}
}
