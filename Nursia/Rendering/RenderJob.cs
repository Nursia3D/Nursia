using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	/// <summary>
	/// Flags that control how a render job is processed.
	/// </summary>
	[Flags]
	public enum RenderJobFlags
	{
		/// <summary>No flags set.</summary>
		None,

		/// <summary>
		/// Determines whether reflection view should be clipped by the reflection plane, if it is specified.
		/// </summary>
		ClipReflectionPlane = 1 << 0,

		/// <summary>
		/// Disables culling by the camera frustum.
		/// </summary>
		DontCullByCameraFrustum = 1 << 1,
	}

	internal abstract class RenderJob
	{
		private Matrix _transform;
		private BoundingBox _localBoundingBox;
		private BoundingBox? _worldBoundingBox;

		public IMaterial Material { get; set; }

		public Matrix Transform
		{
			get => _transform;

			set
			{
				_transform = value;
				_worldBoundingBox = null;
			}
		}

		public RenderJobFlags Flags { get; set; }

		public BoundingBox LocalBoundingBox
		{
			get => _localBoundingBox;

			set
			{
				_localBoundingBox = value;
				_worldBoundingBox = null;
			}
		}

		public BoundingBox WorldBoundingBox
		{
			get
			{
				if (_worldBoundingBox == null)
				{
					var transform = Transform;
					_worldBoundingBox = _localBoundingBox.Transform(ref transform);
				}

				return _worldBoundingBox.Value;
			}
		}

		public Plane? ReflectionPlane { get; set; }
		public Plane? ClipPlane { get; set; }

		internal abstract MaterialTechnique MaterialTechnique { get; }
		internal abstract bool NormalMapping { get; }
		internal RenderTarget2D ReflectionTexture { get; set; }
		internal float SquaredDistanceToCamera { get; set; }
		internal EffectBinding EffectBinding { get; private set; }
		internal int EffectBatchId => EffectBinding.BatchId;
		internal Matrix ModelViewProj { get; set; }


		protected RenderJob()
		{
		}

		public void SetDepthTechnique()
		{
			EffectBinding = UtilityEffects.GetDepthEffect(MaterialTechnique);
		}

		public void SetShadowTechnique()
		{
			EffectBinding = Material.GetShadowTechnique(MaterialTechnique);
		}

		public void SetColorTechnique(LightTechnique lightTechnique, bool shadow, bool translucent)
		{
			EffectBinding = Material.GetColorTechnique(MaterialTechnique, lightTechnique, shadow && Material.Flags.HasFlag(MaterialFlags.AcceptsShadows), translucent, NormalMapping && !Nrs.DebugSettings.DisableNormalMap, ClipPlane != null);
		}

		protected internal abstract void Render(GraphicsDevice graphicsDevice, ref RenderStatistics statistics);

		public virtual void Recycle()
		{
			Material = null;
			ReflectionPlane = null;
			ClipPlane = null;
			ReflectionTexture = null;
			EffectBinding = null;
		}
	}

	internal class RenderJobMesh : RenderJob
	{
		private static readonly ObjectPool<RenderJobMesh> _objectPool = new ObjectPool<RenderJobMesh>(() => new RenderJobMesh());

		private DrMeshPart _mesh;

		public DrMeshPart Mesh
		{
			get => _mesh;

			set
			{
				_mesh = value;

				if (_mesh != null)
				{
					LocalBoundingBox = _mesh.BoundingBox;
				}
			}
		}


		public Matrix[] BonesTransforms { get; set; }

		internal override MaterialTechnique MaterialTechnique => BonesTransforms != null ? MaterialTechnique.Skinned : MaterialTechnique.Ordinary;

		internal override bool NormalMapping => _mesh != null && _mesh.TangentsFormat == VertexElementFormat.Vector4;

		private RenderJobMesh()
		{
		}

		protected internal override void Render(GraphicsDevice device, ref RenderStatistics statistics)
		{
			Mesh.Draw(device, ref statistics);
		}

		public static RenderJobMesh Obtain() => _objectPool.Get();

		public override void Recycle()
		{
			base.Recycle();

			Mesh = null;

			_objectPool.Recycle(this);
		}
	}

	internal class RenderJobMeshInstanced : RenderJob
	{
		private static readonly ObjectPool<RenderJobMeshInstanced> _objectPool = new ObjectPool<RenderJobMeshInstanced>(() => new RenderJobMeshInstanced());

		public DrMeshPart Mesh { get; set; }

		public VertexBuffer InstancesTransforms { get; set; }

		internal override MaterialTechnique MaterialTechnique => MaterialTechnique.Instanced;

		internal override bool NormalMapping => Mesh != null && Mesh.TangentsFormat == VertexElementFormat.Vector4;

		private RenderJobMeshInstanced()
		{
		}

		protected internal override void Render(GraphicsDevice device, ref RenderStatistics statistics)
		{
			device.SetVertexBuffers(new VertexBufferBinding(Mesh.VertexBuffer), new VertexBufferBinding(InstancesTransforms, 0, 1));

			if (Mesh.IndexBuffer == null)
			{
				throw new Exception($"Instanced primitives must be indexed");
			}
			else
			{

				device.Indices = Mesh.IndexBuffer;

#if MONOGAME
				device.DrawInstancedPrimitives(Mesh.PrimitiveType, Mesh.VertexOffset, Mesh.StartIndex, Mesh.PrimitiveCount, InstancesTransforms.VertexCount);
#else
				device.DrawInstancedPrimitives(Mesh.PrimitiveType, Mesh.VertexOffset, 0, Mesh.NumVertices, Mesh.StartIndex, Mesh.PrimitiveCount, InstancesTransforms.VertexCount);
#endif
			}

			statistics.VerticesDrawn += Mesh.NumVertices * InstancesTransforms.VertexCount;
			statistics.PrimitivesDrawn += Mesh.PrimitiveCount * InstancesTransforms.VertexCount;
			++statistics.DrawCalls;
		}

		public static RenderJobMeshInstanced Obtain() => _objectPool.Get();

		public override void Recycle()
		{
			base.Recycle();

			Mesh = null;
			InstancesTransforms = null;

			_objectPool.Recycle(this);
		}
	}
}