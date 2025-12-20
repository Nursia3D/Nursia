using Nursia.Rendering;
using Nursia.SceneGraph.Lights;
using System;

namespace Nursia.SceneGraph
{
	partial class SceneNode : IRenderSource
	{
		private static Action<SceneNode, Camera, ILightsBatch> _lightHandler = new Action<SceneNode, Camera, ILightsBatch>((n, c, b) =>
		{
			var asLight = n as BaseLight;

			if (asLight != null)
			{
				b.AddLight(asLight);
			}
		});
		private static Action<SceneNode, Camera, IRenderJobsBatch> _renderHandler = new Action<SceneNode, Camera, IRenderJobsBatch>((n, c, b) => n.AddRenderJobs(c, b));

		void IRenderSource.QueryLights(Camera camera, ILightsBatch batch)
		{
			Traverse(_lightHandler, camera, batch);
		}

		void IRenderSource.QueryRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			Traverse(_renderHandler, camera, batch);
		}
	}
}
