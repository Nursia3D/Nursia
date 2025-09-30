using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Materials;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.SceneGraph.Water
{
	[EditorInfo("Base")]
	public class WaterNode : SceneNode
	{
		private readonly DrMeshPart _planeMesh;

		[Category("Appearance")]
		public IMaterial Material { get; set; } = new WaterMaterial();

		public override BoundingBox? BoundingBox => _planeMesh.BoundingBox;

		public WaterNode()
		{
			_planeMesh = MeshPrimitives.CreatePlaneMeshPart(Nrs.GraphicsDevice, normalDirection: NormalDirection.UpY);
		}

		protected internal override void Render(IRenderBatch batch)
		{
			base.Render(batch);

			if (Material == null)
			{
				return;
			}

			batch.BatchJob(Material, GlobalTransform, _planeMesh,
				flags: RenderJobFlags.ClipReflectionPlane,
				reflectionPlane: new Plane(Vector3.UnitY, 0));
		}

		protected override SceneNode CreateInstanceCore() => new WaterNode();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var water = (WaterNode)node;

			Material = water.Material;
		}
	}
}
