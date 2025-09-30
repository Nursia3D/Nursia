using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;
using DigitalRiseModel;
using Nursia.Utilities;
using Nursia.Materials;

namespace Nursia.SceneGraph.Billboards
{
	public abstract class BillboardNodeBase : SceneNode
	{
		private static DrMeshPart _mesh;
		private readonly UnlitMaterial _material;

		public Color Color { get; set; } = Color.White;

		private static DrMeshPart Mesh
		{
			get
			{
				if (_mesh == null)
				{
					_mesh = MeshPrimitives.CreatePlaneMeshPart(Nrs.GraphicsDevice);
				}

				return _mesh;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding ShadowMapEffect => null;

		public bool CastsShadows => false;

		public bool AcceptsShadows => false;

		protected float Width
		{
			get => Scale.X;

			set
			{
				var s = Scale;
				s.X = value;
				Scale = s;
			}
		}
		protected float Height
		{
			get => Scale.Y;

			set
			{
				var s = Scale;
				s.Y = value;
				Scale = s;
			}
		}

		protected abstract Texture2D RenderTexture { get; }

		protected BillboardNodeBase()
		{
			_material = new UnlitMaterial
			{
				IsTransparent = true
			};
		}

		protected internal override void Render(IRenderBatch batch)
		{
			base.Render(batch);

			// Apply scale
			var transform = Matrix.CreateScale(Scale);

			// Apply billboard transform
			transform *= Mathematics.CreateScreenAlignedBillboard(batch.Camera.View, Translation);

			// Apply parent transform
			if (Parent != null)
			{
				transform *= Parent.GlobalTransform;
			}

			_material.DiffuseColor = Color;
			_material.Texture = RenderTexture;
			batch.BatchJob(_material, transform, Mesh);
		}
	}
}