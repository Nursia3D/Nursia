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
	/// <summary>
	/// Base class for billboard nodes that render a textured quad facing the camera.
	/// </summary>
	public abstract class BillboardNodeBase : SceneNode
	{
		private static DrMeshPart _mesh;
		private readonly UnlitMaterial _material;

		/// <summary>
		/// Gets the flags for this node.
		/// </summary>
		public override SceneFlags Flags => SceneFlags.HasRenderJobs;

		/// <summary>
		/// Gets or sets the color of the billboard.
		/// </summary>
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

		/// <summary>
		/// Gets the effect binding for shadow mapping (always null for billboards).
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public EffectBinding ShadowMapEffect => null;

		/// <summary>
		/// Gets whether this billboard casts shadows (always false).
		/// </summary>
		public bool CastsShadows => false;

		/// <summary>
		/// Gets whether this billboard accepts shadows (always false).
		/// </summary>
		public bool AcceptsShadows => false;

		/// <summary>
		/// Gets or sets the width of the billboard.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the height of the billboard.
		/// </summary>
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

		public override void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			base.AddRenderJobs(camera, batch);

			// Update material
			_material.DiffuseColor = Color;
			_material.Texture = RenderTexture;

			// Update transform
			var transform = Matrix.CreateScale(Scale);

			// Apply billboard transform
			transform *= Mathematics.CreateScreenAlignedBillboard(camera.View, Translation);

			// Apply parent transform
			if (Parent != null)
			{
				transform *= Parent.GlobalTransform;
			}

			batch.AddMesh(Mesh, _material, transform);
		}
	}
}