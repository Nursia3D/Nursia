using Nursia.Materials;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal enum RenderPassStage
	{
		UnlitOpaque,
		LitOpaque,
		Transparent
	}

	internal interface IPassBatches
	{
		JobsBatch OpaqueUnlit { get; }
		JobsBatch OpaqueLit { get; }
		JobsBatch Transparent { get; }

		int Count { get; }
	}

	internal class RenderBatchMain : RenderBatchBase
	{
		private class PassBatches : IPassBatches
		{
			public JobsBatch OpaqueUnlit { get; } = new JobsBatch();
			public JobsBatch OpaqueLit { get; } = new JobsBatch();
			public JobsBatch Transparent { get; } = new JobsBatch(JobsBatchSortMethod.BackToFront);

			public IEnumerable<RenderJob> AllJobs
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

			public void Reset()
			{
				OpaqueUnlit.Reset();
				OpaqueLit.Reset();
				Transparent.Reset();
			}
		}

		private readonly PassBatches _mainStorage = new PassBatches();
		private readonly PassBatches _secondaryStorage = new PassBatches();

		public IPassBatches MainStorage => _mainStorage;
		public List<RenderJob> Reflections { get; } = new List<RenderJob>();
		public IPassBatches SecondaryStorage => _secondaryStorage;
		public bool RequiresDepthBuffer { get; private set; }
		public bool RequiresScreenTexture { get; private set; }

		public override IEnumerable<RenderJob> AllJobs
		{
			get
			{
				foreach (var job in _mainStorage.AllJobs)
				{
					yield return job;
				}

				foreach (var job in _secondaryStorage.AllJobs)
				{
					yield return job;
				}

				yield break;
			}
		}

		protected override void InternalAddJob(RenderJob job)
		{
			var materialFlags = job.Material.Flags;
			var storage = (materialFlags.HasFlag(MaterialFlags.RequiresDepthBuffer) || materialFlags.HasFlag(MaterialFlags.RequiresScreenTexture) || job.ReflectionPlane != null) ? _secondaryStorage : _mainStorage;

			if (materialFlags.HasFlag(MaterialFlags.RequiresDepthBuffer))
			{
				RequiresDepthBuffer = true;
			}

			if (materialFlags.HasFlag(MaterialFlags.RequiresScreenTexture))
			{
				RequiresScreenTexture = true;
			}

			var batch = storage.GetJobsBatch(materialFlags.HasFlag(MaterialFlags.AcceptsLight), materialFlags.HasFlag(MaterialFlags.IsTransparent));
			batch.AddJob(job);
			if (job.ReflectionPlane != null)
			{
				Reflections.Add(job);
			}
		}

		public override void Reset()
		{
			RequiresDepthBuffer = false;
			RequiresScreenTexture = false;
			_mainStorage.Reset();
			_secondaryStorage.Reset();
			Reflections.Clear();
		}
	}
}
