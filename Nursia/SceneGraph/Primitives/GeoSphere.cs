using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a geodesic sphere shape.
	/// </summary>
	[EditorInfo("Primitives")]
	public class GeoSphere : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private int _tessellation = 3;

		/// <summary>
		/// Gets or sets the radius of the geodesic sphere.
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

		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateGeoSphereMeshPart(Nrs.GraphicsDevice, Radius, Tessellation, UScale, VScale, IsLeftHanded);

		protected override SceneNode CreateInstanceCore() => new GeoSphere();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var sphere = (GeoSphere)node;
			Radius = sphere.Radius;
			Tessellation = sphere.Tessellation;
		}
	}
}