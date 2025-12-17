using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.SceneGraph;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	public interface IRenderBatch
	{
		Camera Camera { get; }

		void BatchJob(IMaterial material, Matrix transform, DrMeshPart mesh,
			Action renderCallback = null, RenderJobFlags flags = RenderJobFlags.None,
			Matrix[] bonesTransforms = null, bool cullByBoundingBox = true,
			Plane? clipPlane = null, Plane? reflectionPlane = null, VertexBuffer instancesTransforms = null,
			BoundingBox? customBox = null);
	}

	internal abstract class RenderBatchBase : IRenderBatch
	{
		public Camera Camera { get; private set; }
		public abstract IEnumerable<RenderJob> AllJobs { get; }

		public void PrepareRender(Camera camera)
		{
			Camera = camera ?? throw new ArgumentNullException(nameof(camera));

			Clear();
		}

		public abstract void BatchJob(IMaterial material, Matrix transform, DrMeshPart mesh,
			Action renderCallback = null, RenderJobFlags flags = RenderJobFlags.None,
			Matrix[] bonesTransforms = null, bool cullByBoundingBox = true,
			Plane? clipPlane = null, Plane? reflectionPlane = null, VertexBuffer instancesTransforms = null,
			BoundingBox? customBox = null);
		public abstract void Clear();
	}
}
