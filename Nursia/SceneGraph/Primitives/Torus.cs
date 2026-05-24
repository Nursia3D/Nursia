using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a torus (donut) shape.
	/// </summary>
	[EditorInfo("Primitives")]
	public class Torus : PrimitiveMeshNode
	{
		private float _majorRadius = 0.5f;
		private float _minorRadius = 0.16666f;
		private int _tessellation = 32;

		/// <summary>
		/// Gets or sets the major radius of the torus.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the minor radius (thickness) of the torus.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the number of tessellation levels for the torus mesh.
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
		/// Creates the torus mesh.
		/// </summary>
		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateTorusMeshPart(Nrs.GraphicsDevice, MajorRadius, MinorRadius, Tessellation, UScale, VScale, IsLeftHanded);

		/// <summary>
		/// Creates a new instance of the Torus class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new Torus();

		/// <summary>
		/// Copies all torus properties from another torus node.
		/// </summary>
		/// <param name="node">The source torus node to copy from.</param>
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