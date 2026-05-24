using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Env;
using Nursia.Materials;
using Nursia.SceneGraph;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal abstract class RenderBatchBase : IRenderJobsBatch
	{
		public Camera Camera { get; private set; }

		public abstract IEnumerable<RenderJob> AllJobs { get; }

		public void BatchScene(IRenderSource source, Camera camera, RenderEnvironment renderEnvironment)
		{
			Reset();

			Camera = camera ?? throw new ArgumentNullException(nameof(camera));

			if (renderEnvironment != null && renderEnvironment.Sky != null)
			{
				renderEnvironment.Sky.AddRenderJobs(camera, this);
			}

			source.QueryRenderJobs(camera, this);

			Camera = null;
		}

		private void AddJob(RenderJob job)
		{
			if (!job.Flags.HasFlag(RenderJobFlags.DontCullByCameraFrustum) && !Camera.Frustum.Intersects(job.WorldBoundingBox))
			{
				job.Recycle();
				return;
			}

			job.SquaredDistanceToCamera = Vector3.DistanceSquared(Camera.Translation, job.WorldBoundingBox.CalculateCenter());
			InternalAddJob(job);
		}

		void IRenderJobsBatch.AddMesh(DrMeshPart mesh, IMaterial material, Matrix transform,
			RenderJobFlags flags, Matrix[] bonesTransforms, Plane? clipPlane, Plane? reflectionPlane)
		{
			var job = RenderJobMesh.Obtain();

			job.Mesh = mesh;
			job.Material = material;
			job.Transform = transform;
			job.Flags = flags;
			job.BonesTransforms = bonesTransforms;
			job.ClipPlane = clipPlane;
			job.ReflectionPlane = reflectionPlane;

			AddJob(job);
		}

		void IRenderJobsBatch.AddInstancedMesh(DrMeshPart mesh, IMaterial material, Matrix transform,
			VertexBuffer instancesTransforms, BoundingBox boundingBox,
			RenderJobFlags flags, Plane? clipPlane, Plane? reflectionPlane)
		{
			var job = RenderJobMeshInstanced.Obtain();

			job.Mesh = mesh;
			job.Material = material;
			job.Transform = transform;
			job.InstancesTransforms = instancesTransforms;
			job.LocalBoundingBox = boundingBox;
			job.Flags = flags;
			job.ClipPlane = clipPlane;
			job.ReflectionPlane = reflectionPlane;

			AddJob(job);
		}

		protected abstract void InternalAddJob(RenderJob job);

		public abstract void Reset();
	}
}
