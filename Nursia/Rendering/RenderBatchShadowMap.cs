using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Nursia.Materials;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal class RenderBatchShadowMap : RenderBatchBase
	{
		private readonly JobsBatch _shadowJobs = new JobsBatch();

		public JobsBatch Batch => _shadowJobs;
		public override IEnumerable<RenderJob> AllJobs => _shadowJobs.Jobs;

		public override void BatchJob(IMaterial material, Matrix transform, DrMeshPart mesh,
			Action renderCallback = null, RenderJobFlags flags = RenderJobFlags.None,
			Matrix[] bonesTransforms = null, bool cullByBoundingBox = true,
			Plane? clipPlane = null, Plane? reflectionPlane = null, Matrix[] instancesTransforms = null)
		{
			var materialFlags = material.Flags;
			if (!materialFlags.HasFlag(MaterialFlags.CastsShadows))
			{
				return;
			}

			var boundingBox = Mathematics.DefaultBox;
			if (mesh != null)
			{
				boundingBox = mesh.BoundingBox.Transform(ref transform);
				if (cullByBoundingBox)
				{
					if (Camera.Frustum.Contains(boundingBox) == ContainmentType.Disjoint)
					{
						return;
					}
				}
			}

			var dist = Vector3.DistanceSquared(Camera.Translation, boundingBox.CalculateCenter());
			_shadowJobs.AddJob(material, transform, mesh, renderCallback, flags,
				boundingBox, dist, bonesTransforms, clipPlane, reflectionPlane, instancesTransforms);
		}

		public override void Clear()
		{
			_shadowJobs.Clear();
		}
	}
}
