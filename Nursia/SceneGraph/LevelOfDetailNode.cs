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
	public class LodEntry
	{
		private SceneNode _node;

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

		public float? MaxScreenSpaceSize { get; set; }

		public LodEntry()
		{
		}

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

		[Browsable(false)]
		[JsonIgnore]
		public List<LodEntry> LodLevels { get; } = new List<LodEntry>();

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

		public override BoundingBox? BoundingBox => GetBoundingBoxRecursive();

		private BoundingBox? GetBoundingBoxRecursive()
		{
			foreach (var entry in LodLevels)
			{
				if (entry.Node?.BoundingBox.HasValue ?? false)
				{
					return entry.Node.BoundingBox;
				}
			}

			return null;
		}

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

		protected override SceneNode CreateInstanceCore() => new LevelOfDetailNode();

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

		protected override SceneNode GetCustomChild() => VisibleLodEntry?.Node;
	}
}
