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

		/// <summary>
		/// Gets or sets the height of the cone.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the number of tessellation levels for the cone mesh.
		/// </summary>
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

		/// <summary>
		/// Creates the cone mesh.
		/// </summary>
		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateConeMeshPart(Nrs.GraphicsDevice, Radius, Height, Tessellation, UScale, VScale, IsLeftHanded);

		/// <summary>
		/// Creates a new instance of the Cone class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new Cone();

		/// <summary>
		/// Copies all cone properties from another cone node.
		/// </summary>
		/// <param name="node">The source cone node to copy from.</param>
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