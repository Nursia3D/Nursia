using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.SceneGraph.Lights;
using System;

namespace Nursia.Rendering
{
	[Flags]
	public enum RenderJobFlags
	{
		None,

		/// <summary>
		/// Determines whether reflection view should be clipped by the reflection plane, if it is specified
		/// </summary>
		ClipReflectionPlane = 1 << 0
	}

	/// <summary>
	/// TODO: Use object pools
	/// </summary>
	internal class RenderJob
	{
		public IMaterial Material { get; set; }
		public Matrix Transform { get; set; }
		public Matrix ModelViewProj { get; set; }
		public DrMeshPart Mesh { get; set; }
		public Action RenderCallback { get; set; }
		public RenderJobFlags Flags { get; set; }
		public Matrix[] BonesTransforms { get; set; }
		public RenderTarget2D ReflectionTexture { get; set; }
		/// <summary>
		/// Bounding Box in World Coordinates
		/// </summary>
		public BoundingBox BoundingBox { get; set; }
		public float SquaredDistanceToCamera { get; set; }
		public Plane? ReflectionPlane { get; set; }
		public Plane? ClipPlane { get; set; }

		public EffectBinding EffectBinding { get; private set; }
		public int EffectBatchId => EffectBinding.BatchId;


		public RenderJob()
		{
		}

		public void SetDepthTechnique()
		{
			EffectBinding = UtilityEffects.GetDepthEffect(Mesh != null && Mesh.Skin != null);
		}

		public void SetShadowTechnique()
		{
			EffectBinding = Material.GetShadowTechnique(Mesh);
		}

		public void SetTechnique(LightTechnique lightTechnique, bool shadow, bool translucent)
		{
			EffectBinding = Material.GetColorTechnique(lightTechnique, shadow && Material.Flags.HasFlag(MaterialFlags.AcceptsShadows), translucent, Mesh, ClipPlane != null);
		}

		public void Reset()
		{
			Material = null;
			Mesh = null;
			RenderCallback = null;
			BonesTransforms = null;
			ReflectionTexture = null;
			EffectBinding = null;
		}
	}
}
