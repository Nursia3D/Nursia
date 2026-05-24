using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a cylinder shape.
	/// </summary>
	[EditorInfo("Primitives")]
	public class Cylinder : PrimitiveMeshNode
	{
		private float _height = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 32;

		/// <summary>
		/// Gets or sets the height of the cylinder.
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
		/// Gets or sets the radius of the cylinder.
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
		/// Gets or sets the number of tessellation levels for the cylinder mesh.
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
		/// Creates the cylinder mesh.
		/// </summary>
		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateCylinderMeshPart(Nrs.GraphicsDevice, Height, Radius, Tessellation, UScale, VScale, IsLeftHanded);

		/// <summary>
		/// Creates a new instance of the Cylinder class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new Cylinder();

		/// <summary>
		/// Copies all cylinder properties from another cylinder node.
		/// </summary>
		/// <param name="node">The source cylinder node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var cylinder = (Cylinder)node;

			Height = cylinder.Height;
			Radius = cylinder.Radius;
			Tessellation = cylinder.Tessellation;
		}
	}
}
