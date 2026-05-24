using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.Serialization;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// Base class for all scene nodes that render a mesh.
	/// </summary>
	public abstract class MeshNodeBase : SceneNode
	{
		/// <summary>
		/// Gets the flags for this node.
		/// </summary>
		public override SceneFlags Flags => SceneFlags.HasRenderJobs;

		/// <summary>
		/// Gets the mesh to render. Derived classes must provide this implementation.
		/// </summary>
		protected abstract DrMeshPart RenderMesh { get; }

		/// <summary>
		/// Gets or sets the material for the mesh.
		/// </summary>
		[DefaultMaterial]
		public IMaterial Material { get; set; }

		/// <summary>
		/// Gets the bounding box of the mesh.
		/// </summary>
		public override BoundingBox? BoundingBox => RenderMesh.BoundingBox;

		/// <summary>
		/// Adds render jobs for the mesh to the specified batch.
		/// </summary>
		/// <param name="camera">The camera used for rendering.</param>
		/// <param name="batch">The render batch to add jobs to.</param>
		public override void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			base.AddRenderJobs(camera, batch);

			if (Material == null || RenderMesh == null)
			{
				return;
			}

			batch.AddMesh(RenderMesh, Material, GlobalTransform);
		}

		/// <summary>
		/// Loads external assets for this node and its material.
		/// </summary>
		/// <param name="assetManager">The asset manager to load resources from.</param>
		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			var hasExternalAssets = Material as IHasExternalAssets;
			if (hasExternalAssets != null)
			{
				hasExternalAssets.Load(assetManager);
			}
		}

		/// <summary>
		/// Copies properties from another mesh node.
		/// </summary>
		/// <param name="node">The source node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var mesh = (MeshNodeBase)node;
			Material = mesh.Material?.Clone();
		}
	}
}
