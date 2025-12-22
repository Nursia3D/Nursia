using Nursia.Materials;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal enum JobsBatchSortMethod
	{
		FrontToBack,
		BackToFront
	}

	internal class JobsBatch
	{
		private class DefaultComparer : IComparer<RenderJob>
		{
			public JobsBatchSortMethod SortMethod { get; }

			public DefaultComparer(JobsBatchSortMethod sortMethod = JobsBatchSortMethod.FrontToBack)
			{
				SortMethod = sortMethod;
			}

			public int Compare(RenderJob x, RenderJob y)
			{
				// Firstly sort by effect id
				if (x.EffectBatchId < y.EffectBatchId)
					return -1;
				if (x.EffectBatchId > y.EffectBatchId)
					return +1;

				// Then by distance to the camera
				if (x.SquaredDistanceToCamera < y.SquaredDistanceToCamera)
				{
					return SortMethod == JobsBatchSortMethod.FrontToBack ? -1 : 1;
				}

				if (x.SquaredDistanceToCamera > y.SquaredDistanceToCamera)
				{
					return SortMethod == JobsBatchSortMethod.FrontToBack ? 1 : -1;
				}

				return 0;
			}
		}

		private readonly DefaultComparer _comparer;


		private bool _jobsSorted = false;

		internal List<RenderJob> UnsortedJobs = new List<RenderJob>();

		public int Count => UnsortedJobs.Count;

		public List<RenderJob> Jobs
		{
			get
			{
				if (!_jobsSorted)
				{
					UnsortedJobs.Sort(_comparer);
					_jobsSorted = true;
				}

				return UnsortedJobs;
			}
		}

		public JobsBatch(JobsBatchSortMethod sortMethod = JobsBatchSortMethod.FrontToBack)
		{
			_comparer = new DefaultComparer(sortMethod);
		}

		public RenderJob AddJob(RenderJob job)
		{
			UnsortedJobs.Add(job);
			_jobsSorted = false;

			return job;
		}

		public void Reset()
		{
			foreach(var job in UnsortedJobs)
			{
				job.Recycle();
			}

			UnsortedJobs.Clear();
			_jobsSorted = false;
		}

		public void SetDepthTechnique()
		{
			foreach (var job in UnsortedJobs)
			{
				job.SetDepthTechnique();
			}
		}

		public void SetShadowTechnique()
		{
			foreach (var job in UnsortedJobs)
			{
				job.SetShadowTechnique();
			}
		}

		public void SetTechnique(LightTechnique lightTechnique, bool shadow, bool translucent)
		{
			foreach (var job in UnsortedJobs)
			{
				job.SetColorTechnique(lightTechnique, shadow, translucent);
			}
		}
	}
}
