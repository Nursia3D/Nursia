using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Env;
using Nursia.Rendering;
using Nursia.SceneGraph.Lights;
using System;

namespace Nursia.SceneGraph
{
	public class SceneRenderer : IRenderSource
	{
		private readonly ForwardRenderer _renderer = new ForwardRenderer();

		public Scene Scene { get; }

		public SceneNode Root => Scene.Root;
		public Camera Camera => Scene.Camera;
		public RenderEnvironment RenderEnvironment => Scene.RenderEnvironment;

		public RenderStatistics Statistics => _renderer.Statistics;

		public SceneRenderer(Scene scene)
		{
			Scene = scene ?? throw new ArgumentNullException(nameof(scene));
		}

		public SceneRenderer(SceneNode root, Camera camera, RenderEnvironment renderEnvironment)
		{
			Scene = new Scene(root, camera, renderEnvironment);
		}

		public SceneRenderer(SceneNode root, Camera camera) : this(root, camera, RenderEnvironment.Default.Clone())
		{
		}

		public SceneRenderer(SceneNode root) : this(root, new Camera())
		{
		}

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

		public void Render(Camera camera, RenderEnvironment renderEnvironment)
		{
			_renderer.Render(this, camera, renderEnvironment);
		}

		public void Render(Camera camera) => Render(camera, RenderEnvironment);

		public void Render() => Render(Camera, RenderEnvironment);

		public RenderTarget2D RenderToTarget(Camera camera, RenderEnvironment renderEnvironment, int width, int height)
		{
			return _renderer.RenderToTarget(this, camera, renderEnvironment, width, height);
		}

		public RenderTarget2D RenderToTarget(Camera camera, int width, int height) => RenderToTarget(camera, RenderEnvironment, width, height);

		public RenderTarget2D RenderToTarget(int width, int height) => RenderToTarget(Camera, RenderEnvironment, width, height);
	}
}
