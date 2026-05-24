using DigitalRiseModel;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// A scene node that renders a single mesh part.
	/// </summary>
	public class MeshNode : MeshNodeBase
	{
		/// <summary>
		/// Gets or sets the mesh part to render.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public DrMeshPart Mesh { get; set; }

		/// <summary>
		/// Gets the mesh part to render.
		/// </summary>
		protected override DrMeshPart RenderMesh => Mesh;

		/// <summary>
		/// Creates a new instance of the MeshNode class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new MeshNode();

		/// <summary>
		/// Copies all mesh properties from another mesh node.
		/// </summary>
		/// <param name="node">The source mesh node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var mesh = (MeshNode)node;
			Mesh = mesh.Mesh;
		}
	}
}
