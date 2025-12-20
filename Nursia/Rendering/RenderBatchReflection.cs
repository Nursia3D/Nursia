using Microsoft.Xna.Framework;
using Nursia.Materials;
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

		protected override void InternalAddJob(RenderJob job)
		{
			if (job.ReflectionPlane != null)
			{
				// Ignore other reflective jobs for this batch
				job.Recycle();
				return;
			}

			// Use batch defined clip plane
			var materialFlags = job.Material.Flags;
			var batch = GetJobsBatch(materialFlags.HasFlag(MaterialFlags.AcceptsLight), materialFlags.HasFlag(MaterialFlags.IsTransparent));

			job.ClipPlane = ClipPlane;
			batch.AddJob(job);
		}

		public override void Reset()
		{
			OpaqueUnlit.Reset();
			OpaqueLit.Reset();
			Transparent.Reset();
		}
	}
}
