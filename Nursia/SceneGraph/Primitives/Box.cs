using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a box shape.
	/// </summary>
	[EditorInfo("Primitives")]
	public class Box : PrimitiveMeshNode
	{
		private Vector3 _size = Vector3.One;

		/// <summary>
		/// Gets or sets the size of the box.
		/// </summary>
		[Category("Geometry")]
		public Vector3 Size
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

		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateBoxMeshPart(Nrs.GraphicsDevice, Size, UScale, VScale, IsLeftHanded);

		protected override SceneNode CreateInstanceCore() => new Box();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var box = (Box)node;

			Size = box.Size;
		}
	}
}