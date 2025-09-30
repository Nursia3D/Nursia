using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Nursia.Materials;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal class RenderBatchReflection : RenderBatchBase, IPassBatches
	{
		public JobsBatch OpaqueUnlit { get; } = new JobsBatch();
		public JobsBatch OpaqueLit { get; } = new JobsBatch();
		public JobsBatch Transparent { get; } = new JobsBatch(JobsBatchSortMethod.BackToFront);

		public override IEnumerable<RenderJob> AllJobs
		{
			get
			{
				foreach (var job in OpaqueUnlit.Jobs)
				{
					yield return job;
				}

				foreach (var job in OpaqueLit.Jobs)
				{
					yield return job;
				}

				foreach (var job in Transparent.Jobs)
				{
					yield return job;
				}

				yield break;
			}
		}

		public int Count => OpaqueUnlit.Count + OpaqueLit.Count + Transparent.Count;
		public Plane? ClipPlane { get; set; }

		public JobsBatch GetJobsBatch(bool receivesLight, bool isTransparent)
		{
			if (!receivesLight && !isTransparent)
			{
				return OpaqueUnlit;
			}
			else if (!receivesLight && isTransparent)
			{
				return Transparent;
			}
			else if (receivesLight && !isTransparent)
			{
				return OpaqueLit;
			}

			throw new NotSupportedException("Materials with ReceivesLight=true and IsTransparent=true arent supported");
		}

		public override void BatchJob(IMaterial material, Matrix transform, DrMeshPart mesh,
			Action renderCallback = null, RenderJobFlags flags = RenderJobFlags.None,
			Matrix[] bonesTransforms = null, bool cullByBoundingBox = true,
			Plane? clipPlane = null, Plane? reflectionPlane = null)
		{
			if (reflectionPlane != null)
			{
				// Ignore other reflective jobs for this batch
				return;
			}

			var boundingBox = mesh.BoundingBox.Transform(ref transform);
			if (cullByBoundingBox)
			{
				if (Camera.Frustum.Contains(boundingBox) == ContainmentType.Disjoint)
				{
					return;
				}
			}

			// Use batch defined clip plane
			var dist = Vector3.DistanceSquared(Camera.Translation, boundingBox.CalculateCenter());

			var materialFlags = material.Flags;
			var batch = GetJobsBatch(materialFlags.HasFlag(MaterialFlags.AcceptsLight), materialFlags.HasFlag(MaterialFlags.IsTransparent));
			batch.AddJob(material, transform, mesh, renderCallback, flags,
				boundingBox, dist, bonesTransforms, ClipPlane, null);
		}

		public override void Clear()
		{
			OpaqueUnlit.Clear();
			OpaqueLit.Clear();
			Transparent.Clear();
		}
	}
}
