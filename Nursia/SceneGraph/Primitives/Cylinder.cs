using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	[EditorInfo("Primitives")]
	public class Cylinder : PrimitiveMeshNode
	{
		private float _height = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 32;

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

		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateCylinderMeshPart(Nrs.GraphicsDevice, Height, Radius, Tessellation, UScale, VScale, IsLeftHanded);

		protected override SceneNode CreateInstanceCore() => new Cylinder();

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
