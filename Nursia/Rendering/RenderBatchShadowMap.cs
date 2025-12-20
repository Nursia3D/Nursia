using Nursia.Materials;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal class RenderBatchShadowMap : RenderBatchBase
	{
		private readonly JobsBatch _shadowJobs = new JobsBatch();

		public JobsBatch Batch => _shadowJobs;
		public override IEnumerable<RenderJob> AllJobs => _shadowJobs.Jobs;

		protected override void InternalAddJob(RenderJob job)
		{
			var materialFlags = job.Material.Flags;
			if (!materialFlags.HasFlag(MaterialFlags.CastsShadows))
			{
				job.Recycle();
				return;
			}

			_shadowJobs.AddJob(job);
		}

		public override void Reset()
		{
			_shadowJobs.Reset();
		}
	}
}
