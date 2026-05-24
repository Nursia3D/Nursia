using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// Represents a level of detail entry in a level of detail node.
	/// </summary>
	public class LodEntry
	{
		private SceneNode _node;

		/// <summary>
		/// Gets or sets the scene node for this level of detail.
		/// </summary>
		public SceneNode Node
		{
			get => _node;

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_node = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum screen space size at which this LOD level is displayed.
		/// If null, this LOD is always used.
		/// </summary>
		public float? MaxScreenSpaceSize { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LodEntry"/> class.
		/// </summary>
		public LodEntry()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LodEntry"/> class with the specified node and screen space size.
		/// </summary>
		/// <param name="node">The scene node for this LOD level.</param>
		/// <param name="maxScreenSpaceSize">The maximum screen space size for this LOD level.</param>
		public LodEntry(SceneNode node, float? maxScreenSpaceSize = null)
		{
			Node = node;
			MaxScreenSpaceSize = maxScreenSpaceSize;
		}
	}

	/// <summary>
	/// Level of Detail node that selects which child to render based on screen space size
	/// </summary>
	[EditorInfo("Optimization")]
	public class LevelOfDetailNode : SceneNode
	{
		private LodEntry _visibleLodEntry = null;

		/// <summary>
		/// Gets the list of LOD level entries.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public List<LodEntry> LodLevels { get; } = new List<LodEntry>();

		/// <summary>
		/// Gets or sets the size ratio between consecutive LOD levels.
		/// </summary>
		public float LodLevelSize { get; set; } = 0.5f;

		private LodEntry VisibleLodEntry
		{
			get => _visibleLodEntry;

			set
			{
				if (ReferenceEquals(value, _visibleLodEntry))
				{
					return;
				}

				_visibleLodEntry = value;

				InvalidateActualChildren();
			}
		}

		/// <summary>
		/// Gets the bounding box of the currently visible LOD level.
		/// </summary>
		public override BoundingBox? BoundingBox => GetBoundingBoxRecursive();

		private BoundingBox? GetBoundingBoxRecursive()
		{
			foreach (var entry in LodLevels)
			{
				if (entry.Node?.BoundingBox != null)
				{
					return entry.Node.BoundingBox;
				}
			}

			return null;
		}

		/// <summary>
		/// Adds render jobs for the visible LOD level based on screen space size.
		/// </summary>
		/// <param name="camera">The camera used for rendering.</param>
		/// <param name="batch">The render batch to add jobs to.</param>
		public override void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			base.AddRenderJobs(camera, batch);

			if (LodLevels.Count == 0)
			{
				VisibleLodEntry = null;
				return;
			}

			var bbox = GetBoundingBoxRecursive();
			if (bbox == null)
			{
				VisibleLodEntry = null;
				return;
			}

			var screenSpaceSize = CalculateScreenSpaceSize(bbox.Value, camera);

			int? lodLevel = null;
			for (int i = LodLevels.Count - 1; i >= 0; i--)
			{
				var entry = LodLevels[i];

				var maxScreenSpaceSize = entry.MaxScreenSpaceSize ?? (float)Math.Pow(LodLevelSize, i + 1);
				if (screenSpaceSize <= maxScreenSpaceSize)
				{
					lodLevel = i;
					break;
				}
			}

			if (lodLevel == null)
			{
				lodLevel = 0;
			}

			VisibleLodEntry = LodLevels[lodLevel.Value];
		}


		/// <summary>
		/// Calculates the screen space size of a bounding box as seen from the camera.
		/// </summary>
		/// <param name="boundingBox">The bounding box to calculate size for.</param>
		/// <param name="camera">The camera to calculate from.</param>
		/// <returns>The screen space size (0 to 1 range).</returns>
		private float CalculateScreenSpaceSize(BoundingBox boundingBox, Camera camera)
		{
			var size = boundingBox.Max - boundingBox.Min;
			var radius = size.Length() * 0.5f;

			var center = boundingBox.CalculateCenter();
			var centerWorld = Vector3.Transform(center, GlobalTransform);
			var cameraPos = camera.GlobalTransform.Translation;
			var distance = Vector3.Distance(centerWorld, cameraPos);

			if (distance <= 0)
			{
				return float.MaxValue;
			}

			var fov = camera.ViewAngle * MathHelper.Pi / 180f;
			var screenSpaceRadius = radius / distance / (float)Math.Tan(fov * 0.5f);

			return screenSpaceRadius;
		}

		/// <summary>
		/// Creates a new instance of the LevelOfDetailNode class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new LevelOfDetailNode();

		/// <summary>
		/// Copies all LOD properties from another level of detail node.
		/// </summary>
		/// <param name="node">The source LOD node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var lodNode = (LevelOfDetailNode)node;

			LodLevels.Clear();
			foreach (var entry in lodNode.LodLevels)
			{
				LodLevels.Add(new LodEntry
				{
					Node = entry.Node?.Clone(),
					MaxScreenSpaceSize = entry.MaxScreenSpaceSize
				});
			}
		}

		/// <summary>
		/// Gets the currently visible custom child node based on LOD selection.
		/// </summary>
		protected override SceneNode GetCustomChild() => VisibleLodEntry?.Node;
	}
}
