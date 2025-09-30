using DigitalRiseModel;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	public class MeshNode : MeshNodeBase
	{
		[Browsable(false)]
		[JsonIgnore]
		public DrMeshPart Mesh { get; set; }

		protected override DrMeshPart RenderMesh => Mesh;

		protected override SceneNode CreateInstanceCore() => new MeshNode();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var mesh = (MeshNode)node;
			Mesh = mesh.Mesh;
		}
	}
}
