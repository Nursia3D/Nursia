using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a capsule shape.
	/// </summary>
	[EditorInfo("Primitives")]
	public class Capsule : PrimitiveMeshNode
	{
		private float _length = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 8;

		/// <summary>
		/// Gets or sets the length of the capsule.
		/// </summary>
		[Category("Geometry")]
		public float Length
		{
			get => _length;

			set
			{
				if (value.EpsilonEquals(_length))
				{
					return;
				}

				_length = value;
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

		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateCapsuleMeshPart(Nrs.GraphicsDevice, Length, Radius, Tessellation, UScale, VScale, IsLeftHanded);

		protected override SceneNode CreateInstanceCore() => new Capsule();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var capsule = (Capsule)node;

			Length = capsule.Length;
			Radius = capsule.Radius;
			Tessellation = capsule.Tessellation;
		}
	}
}