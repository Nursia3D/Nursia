using DigitalRiseModel;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Primitives
{
	/// <summary>
	/// Base class for all primitive geometry nodes.
	/// </summary>
	public abstract class PrimitiveMeshNode : MeshNodeBase
	{
		private bool _isLeftHanded;
		private DrMeshPart _mesh;
		private float _uScale = 1.0f;
		private float _vScale = 1.0f;

		/// <summary>
		/// Gets the render mesh, creating it if necessary.
		/// </summary>
		protected override DrMeshPart RenderMesh
		{
			get
			{
				if (_mesh == null)
				{
					_mesh = CreateMesh();
				}

				return _mesh;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the mesh uses left-handed winding.
		/// </summary>
		[Category("Geometry")]
		public bool IsLeftHanded
		{
			get => _isLeftHanded;

			set
			{
				if (value == _isLeftHanded)
				{
					return;
				}

				_isLeftHanded = value;
				InvalidateMesh();
			}
		}

		/// <summary>
		/// Gets or sets the U coordinate scale for texture mapping.
		/// </summary>
		[Category("Geometry")]
		public float UScale
		{
			get => _uScale;

			set
			{
				if (value.EpsilonEquals(_uScale))
				{
					return;
				}

				_uScale = value;
				InvalidateMesh();
			}
		}

		/// <summary>
		/// Gets or sets the V coordinate scale for texture mapping.
		/// </summary>
		[Category("Geometry")]
		public float VScale
		{
			get => _vScale;

			set
			{
				if (value.EpsilonEquals(_vScale))
				{
					return;
				}

				_vScale = value;
				InvalidateMesh();
			}
		}

		/// <summary>
		/// Creates the mesh geometry for this primitive. Derived classes must implement this.
		/// </summary>
		/// <returns>A new mesh part for the primitive.</returns>
		protected abstract DrMeshPart CreateMesh();

		/// <summary>
		/// Invalidates the cached mesh, causing it to be regenerated on next access.
		/// </summary>
		public void InvalidateMesh()
		{
			_mesh = null;
		}

		/// <summary>
		/// Copies all properties from another primitive mesh node to this node.
		/// </summary>
		/// <param name="node">The source node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var primitive = (PrimitiveMeshNode)node;

			IsLeftHanded = primitive.IsLeftHanded;
			UScale = primitive.UScale;
			VScale = primitive.VScale;
		}
	}
}