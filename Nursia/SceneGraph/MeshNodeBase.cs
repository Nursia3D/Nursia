using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.Serialization;

namespace Nursia.SceneGraph
{
	public abstract class MeshNodeBase : SceneNode
	{
		public override SceneFlags Flags => SceneFlags.HasRenderJobs;

		protected abstract DrMeshPart RenderMesh { get; }

		[DefaultMaterial]
		public IMaterial Material { get; set; }

		public override BoundingBox? BoundingBox => RenderMesh.BoundingBox;

		public override void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			base.AddRenderJobs(camera, batch);

			if (Material == null || RenderMesh == null)
			{
				return;
			}

			batch.AddMesh(RenderMesh, Material, GlobalTransform);
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			var hasExternalAssets = Material as IHasExternalAssets;
			if (hasExternalAssets != null)
			{
				hasExternalAssets.Load(assetManager);
			}
		}

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var mesh = (MeshNodeBase)node;
			Material = mesh.Material?.Clone();
		}
	}
}
