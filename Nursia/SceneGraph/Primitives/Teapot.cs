using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	[EditorInfo("Primitives")]
	public class Teapot : PrimitiveMeshNode
	{
		private float _size = 1.0f;
		private int _tessellation = 8;

		[Category("Geometry")]
		public float Size
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

		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateTeapotMeshPart(Nrs.GraphicsDevice, Size, Tessellation, UScale, VScale, IsLeftHanded);

		protected override SceneNode CreateInstanceCore() => new Teapot();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var teapot = (Teapot)node;
			Size = teapot.Size;
			Tessellation = teapot.Tessellation;
		}
	}
}