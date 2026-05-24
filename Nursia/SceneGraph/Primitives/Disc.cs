using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// A primitive mesh node that renders a disc or circular sector.
	/// </summary>
	[EditorInfo("Primitives")]
	public class Disc : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private float _sectorAngle = 360;
		private int _tessellation = 16;

		/// <summary>
		/// Gets or sets the radius of the disc.
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
		/// Gets or sets the sector angle in degrees for the disc.
		/// </summary>
		[Category("Geometry")]
		public float SectorAngle
		{
			get => _sectorAngle;

			set
			{
				if (value.EpsilonEquals(_sectorAngle))
				{
					return;
				}

				_sectorAngle = value;
				InvalidateMesh();
			}
		}

		/// <summary>
		/// Gets or sets the number of tessellation levels for the disc mesh.
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
		/// Creates the disc mesh.
		/// </summary>
		protected override DrMeshPart CreateMesh() => MeshPrimitives.CreateDiscMeshPart(Nrs.GraphicsDevice, Radius, MathHelper.ToRadians(SectorAngle), Tessellation, UScale, VScale, IsLeftHanded);

		/// <summary>
		/// Creates a new instance of the Disc class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new Disc();

		/// <summary>
		/// Copies all disc properties from another disc node.
		/// </summary>
		/// <param name="node">The source disc node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var disc = (Disc)node;
			Radius = disc.Radius;
			SectorAngle = disc.SectorAngle;
			Tessellation = disc.Tessellation;
		}
	}
}
