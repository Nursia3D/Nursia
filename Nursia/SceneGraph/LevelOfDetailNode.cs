using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
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
		private LodEntry _visibleLodEntry = null;

		[Browsable(false)]
		[JsonIgnore]
		public ObservableCollection<LodEntry> LodLevels { get; } = new ObservableCollection<LodEntry>();

		public LodEntry VisibleLodEntry
		{
			get => _visibleLodEntry;

			set
			{
				if (ReferenceEquals(value, _visibleLodEntry))
				{
					return;
				}

				_visibleLodEntry = value;

				InvalidateChildren();

			}
		}

		[Browsable(false)]
		protected internal override IReadOnlyCollection<SceneNode> ActualChildren
		{
			get
			{
				if (_childrenDirty)
				{
					_actualChildren.Clear();

					if (_visibleLodEntry != null)
					{
						_actualChildren.Add(_visibleLodEntry.Node);
					}

					_actualChildren.AddRange(Children);
					_childrenDirty = false;
				}

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

			LodEntry visibleLodEntry = null;
			for (int i = LodLevels.Count - 1; i >= 0; i--)
			{
				var entry = LodLevels[i];
				if (screenSpaceSize > entry.MaxScreenSpaceSize)
				{
					visibleLodEntry = entry;
					break;
				}
			}

			if (visibleLodEntry == null && LodLevels.Count > 0)
			{
				visibleLodEntry = LodLevels[LodLevels.Count - 1];
			}

			VisibleLodEntry = visibleLodEntry;
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

		protected override void OnChildrenChanged()
		{
			base.OnChildrenChanged();

			InvalidateChildren();
		}

		private void InvalidateChildren()
		{
			_childrenDirty = true;
		}
	}
}
