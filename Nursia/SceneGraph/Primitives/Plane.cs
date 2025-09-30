using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	[EditorInfo("Primitives")]
	public class Plane : PrimitiveMeshNode
	{
		private Vector2 _size = Vector2.One;
		private Point _tessellation = new Point(1, 1);
		private bool _generateBackFace;
		private NormalDirection _normalDirection;

		[Category("Geometry")]
		public Vector2 Size
		{
			get => _size;

			set
			{
				if (value.EpsilonEquals(_size))
				{
					return;
				}

				_size = value;
				InvalidateMesh();
			}
		}

		[Category("Geometry")]
		public Point Tessellation
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

		[Category("Geometry")]
		public bool GenerateBackface
		{
			get => _generateBackFace;

			set
			{
				if (value == _generateBackFace)
				{
					return;
				}

				_generateBackFace = value;
				InvalidateMesh();
			}
		}

		[Category("Geometry")]
		public NormalDirection NormalDirection
		{
			get => _normalDirection;

			set
			{
				if (value == _normalDirection)
				{
					return;
				}

				_normalDirection = value;
				InvalidateMesh();
			}
		}

		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreatePlaneMeshPart(Nrs.GraphicsDevice, Size.X, Size.Y, Tessellation.X, Tessellation.Y, UScale, VScale, GenerateBackface, IsLeftHanded, NormalDirection);

		protected override SceneNode CreateInstanceCore() => new Plane();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var plane = (Plane)node;
			Size = plane.Size;
			Tessellation = plane.Tessellation;
			GenerateBackface = plane.GenerateBackface;
			NormalDirection = plane.NormalDirection;
		}
	}
}