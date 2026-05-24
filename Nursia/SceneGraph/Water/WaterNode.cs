using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Materials;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.SceneGraph.Water
{
	/// <summary>
	/// A scene node that renders a water surface with realistic water rendering.
	/// </summary>
	[EditorInfo("Base")]
	public class WaterNode : SceneNode
	{
		private readonly DrMeshPart _planeMesh;

		/// <summary>
		/// Gets the flags for this node.
		/// </summary>
		public override SceneFlags Flags => SceneFlags.HasRenderJobs;

		/// <summary>
		/// Gets or sets the water material used for rendering.
		/// </summary>
		[Category("Appearance")]
		public IMaterial Material { get; set; } = new WaterMaterial();

		/// <summary>
		/// Gets the bounding box of the water surface.
		/// </summary>
		public override BoundingBox? BoundingBox => _planeMesh.BoundingBox;

		/// <summary>
		/// Initializes a new instance of the <see cref="WaterNode"/> class.
		/// </summary>
		public WaterNode()
		{
			_planeMesh = MeshPrimitives.CreatePlaneMeshPart(Nrs.GraphicsDevice, normalDirection: NormalDirection.UpY);
		}

		/// <summary>
		/// Adds render jobs for the water surface to the specified batch.
		/// </summary>
		/// <param name="camera">The camera used for rendering.</param>
		/// <param name="batch">The render batch to add jobs to.</param>
		public override void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			base.AddRenderJobs(camera, batch);

			if (Material == null)
			{
				return;
			}

			batch.AddMesh(_planeMesh, Material, GlobalTransform,
				flags: RenderJobFlags.ClipReflectionPlane,
				reflectionPlane: new Plane(Vector3.UnitY, 0));
		}

		/// <summary>
		/// Creates a new instance of the WaterNode class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new WaterNode();

		/// <summary>
		/// Copies all properties from another water node to this node.
		/// </summary>
		/// <param name="node">The source node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var water = (WaterNode)node;

			Material = water.Material;
		}
	}
}
