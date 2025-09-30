using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.Serialization;
using Nursia.Utilities;
using System;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	public class RenderCallbackNode: SceneNode
	{
		public Action RenderCallback { get; set; }

		[Category("Appearance")]
		public IMaterial Material { get; set; }

		protected internal override void Render(IRenderBatch batch)
		{
			base.Render(batch);

			if (Material == null || RenderCallback == null)
			{
				return;
			}

			batch.BatchJob(Material, GlobalTransform, null, RenderCallback, cullByBoundingBox: false);
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
