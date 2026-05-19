using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		public float MaxScreenSpaceSize { get; set; }

		public LodEntry()
		{
		}

		public LodEntry(SceneNode node, float maxScreenSpaceSize)
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
		private readonly List<SceneNode> _actualChildren = new List<SceneNode>();
		private bool _childrenDirty = true;

		[Browsable(false)]
		[JsonIgnore]
		public ObservableCollection<LodEntry> LodLevels { get; } = new ObservableCollection<LodEntry>();

		[Browsable(false)]
		protected internal override IReadOnlyCollection<SceneNode> ActualChildren
		{
			get
			{
				UpdateActualChildren();
				return _actualChildren;
			}
		}

		public override BoundingBox? BoundingBox => GetBoundingBoxRecursive();

		public LevelOfDetailNode()
		{
			LodLevels.CollectionChanged += (s, e) => _childrenDirty = true;
		}

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
				return;
			}

			float screenSpaceSize = float.MaxValue;

			if (camera != null)
			{
				var bbox = GetBoundingBoxRecursive();
				if (bbox.HasValue)
				{
					screenSpaceSize = CalculateScreenSpaceSize(bbox.Value, camera);
				}
			}

			SceneNode selectedLod = null;
			for (int i = LodLevels.Count - 1; i >= 0; i--)
			{
				var entry = LodLevels[i];
				if (screenSpaceSize > entry.MaxScreenSpaceSize)
				{
					selectedLod = entry.Node;
					break;
				}
			}

			if (selectedLod == null && LodLevels.Count > 0)
			{
				selectedLod = LodLevels[LodLevels.Count - 1].Node;
			}

			selectedLod.AddRenderJobs(camera, batch);
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

		private void UpdateActualChildren()
		{
			if (!_childrenDirty)
			{
				return;
			}

			_actualChildren.Clear();
			_actualChildren.AddRange(from e in LodLevels select e.Node);
			_actualChildren.AddRange(Children);
			_childrenDirty = false;
		}

		protected override void OnChildrenChanged()
		{
			base.OnChildrenChanged();

			_childrenDirty = true;
		}
	}
}
