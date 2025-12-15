using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Nursia.Materials;
using Nursia.SceneGraph.Lights;
using Nursia.Utilities;
using System;
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
		private readonly ObjectPool<RenderJob> _renderJobsPool = new ObjectPool<RenderJob>(() => new RenderJob());

		internal List<RenderJob> UnsortedJobs = new List<RenderJob>();

		public int Count => UnsortedJobs.Count;

		internal List<RenderJob> Jobs
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

		/// <summary>
		/// Gets a job from the render pool and adds it to the list
		/// </summary>
		public RenderJob AddJob(IMaterial material, Matrix transform, DrMeshPart mesh,
			Action renderCallback, RenderJobFlags flags, BoundingBox boundingBox,
			float squaredDistanceToViewer, Matrix[] bonesTransforms,
			Plane? clipPlane, Plane? reflectionPlane)
		{
			if (material == null)
			{
				throw new ArgumentNullException(nameof(material));
			}

			if (mesh == null && renderCallback == null)
			{
				throw new ArgumentException("Either mesh or renderCallback shouldn't be null");
			}

			var job = _renderJobsPool.Get();

			job.Material = material;
			job.Transform = transform;
			job.Mesh = mesh;
			job.RenderCallback = renderCallback;
			job.Flags = flags;
			job.BoundingBox = boundingBox;
			job.SquaredDistanceToCamera = squaredDistanceToViewer;
			job.BonesTransforms = bonesTransforms;
			job.ClipPlane = clipPlane;
			job.ReflectionPlane = reflectionPlane;

			UnsortedJobs.Add(job);
			_jobsSorted = false;

			return job;
		}

		public void Clear()
		{
			foreach (var job in UnsortedJobs)
			{
				// Return job to the object pool
				job.Reset();
				_renderJobsPool.Recycle(job);
			}

			UnsortedJobs.Clear();
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
				job.SetTechnique(lightTechnique, shadow, translucent);
			}
		}
	}
}
