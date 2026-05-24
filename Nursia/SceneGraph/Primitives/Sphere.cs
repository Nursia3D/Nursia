using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a sphere shape.
	/// </summary>
	[EditorInfo("Primitives")]
	public class Sphere : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private int _tessellation = 16;

		/// <summary>
		/// Gets or sets the radius of the sphere.
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
		/// Gets or sets the number of tessellation levels for the sphere mesh.
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
		/// Creates the sphere mesh.
		/// </summary>
		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateSphereMeshPart(Nrs.GraphicsDevice, Radius, Tessellation, UScale, VScale, IsLeftHanded);

		/// <summary>
		/// Creates a new instance of the Sphere class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new Sphere();

		/// <summary>
		/// Copies all sphere properties from another sphere node.
		/// </summary>
		/// <param name="node">The source sphere node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var sphere = (Sphere)node;
			Radius = sphere.Radius;
			Tessellation = sphere.Tessellation;
		}
	}
}