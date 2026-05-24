using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a teapot shape.
	/// </summary>
	[EditorInfo("Primitives")]
	public class Teapot : PrimitiveMeshNode
	{
		private float _size = 1.0f;
		private int _tessellation = 8;

		/// <summary>
		/// Gets or sets the size of the teapot.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the number of tessellation levels for the teapot mesh.
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
		/// Creates the teapot mesh.
		/// </summary>
		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateTeapotMeshPart(Nrs.GraphicsDevice, Size, Tessellation, UScale, VScale, IsLeftHanded);

		/// <summary>
		/// Creates a new instance of the Teapot class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new Teapot();

		/// <summary>
		/// Copies all teapot properties from another teapot node.
		/// </summary>
		/// <param name="node">The source teapot node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var teapot = (Teapot)node;
			Size = teapot.Size;
			Tessellation = teapot.Tessellation;
		}
	}
}