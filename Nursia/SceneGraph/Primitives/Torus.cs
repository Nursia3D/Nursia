using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	[EditorInfo("Primitives")]
	public class Torus : PrimitiveMeshNode
	{
		private float _majorRadius = 0.5f;
		private float _minorRadius = 0.16666f;
		private int _tessellation = 32;

		[Category("Geometry")]
		public float MajorRadius
		{
			get => _majorRadius;

			set
			{
				if (value.EpsilonEquals(_majorRadius))
				{
					return;
				}

				_majorRadius = value;
				InvalidateMesh();
			}
		}

		[Category("Geometry")]
		public float MinorRadius
		{
			get => _minorRadius;

			set
			{
				if (value.EpsilonEquals(_minorRadius))
				{
					return;
				}

				_minorRadius = value;
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

		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateTorusMeshPart(Nrs.GraphicsDevice, MajorRadius, MinorRadius, Tessellation, UScale, VScale, IsLeftHanded);

		protected override SceneNode CreateInstanceCore() => new Torus();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var torus = (Torus)node;
			MajorRadius = torus.MajorRadius;
			MinorRadius = torus.MinorRadius;
			Tessellation = torus.Tessellation;
		}
	}
}