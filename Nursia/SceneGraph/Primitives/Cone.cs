using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a cone shape.
	/// </summary>
	[EditorInfo("Primitives")]
	public class Cone : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private float _height = 1.0f;
		private int _tessellation = 16;

		/// <summary>
		/// Gets or sets the radius of the cone base.
		/// </summary>
		[Category("Geometry")]
		public float Radius
		{
			get => _radius;

			set
			{
				if (value.EpsilonEquals(_radius))
				{
					return;
				}

				_radius = value;
				InvalidateMesh();
			}
		}

		[Category("Geometry")]
		public float Height
		{
			get => _height;

			set
			{
				if (value.EpsilonEquals(_height))
				{
					return;
				}

				_height = value;
				InvalidateMesh();
			}
		}

		[Category("Geometry")]
		public int Tessellation
		{
			get => _tessellation;

			set
			{
				if (value == _tessellation)
				{
					return;
				}

				_tessellation = value;
				InvalidateMesh();
			}
		}

		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateConeMeshPart(Nrs.GraphicsDevice, Radius, Height, Tessellation, UScale, VScale, IsLeftHanded);

		protected override SceneNode CreateInstanceCore() => new Cone();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var cone = (Cone)node;
			Radius = cone.Radius;
			Height = cone.Height;
			Tessellation = cone.Tessellation;
		}
	}
}