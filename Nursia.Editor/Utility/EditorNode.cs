using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.SceneGraph;
using System;

namespace Nursia.Editor.Utility
{
	internal class EditorNode : SceneNode
	{
		public DrMeshPart Mesh { get; }
		public IMaterial Material { get; }
		public Matrix Transform { get; set; }

		public EditorNode(DrMeshPart mesh, IMaterial material)
		{
			Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
			Material = material ?? throw new ArgumentNullException(nameof(material));
		}

		protected internal override void Render(IRenderBatch batch)
		{
			base.Render(batch);

			batch.BatchJob(Material, Transform, Mesh);
		}
	}
}
