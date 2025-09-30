using AssetManagementBase;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	[EditorInfo("Base")]
	public class SubsceneNode : SceneNode
	{
		private SceneNode _node;
		private readonly List<SceneNode> _actualChildren = new List<SceneNode>();
		private bool _childrenDirty = true;

		[JsonIgnore]
		[Category("Resources")]
		public SceneNode Node
		{
			get => _node;

			set
			{
				if (value == _node)
				{
					return;
				}

				if (_node != null)
				{
					_node.Parent = null;
				}

				_node = value;
				_childrenDirty = true;

				if (_node != null)
				{
					_node.Parent = this;
				}
			}
		}

		[Browsable(false)]
		public string NodePath { get; set; }

		public override BoundingBox? BoundingBox
		{
			get
			{
				if (Node != null)
				{
					return Node.BoundingBox;
				}

				return base.BoundingBox;
			}
		}
		protected internal override IReadOnlyCollection<SceneNode> ActualChildren
		{
			get
			{
				UpdateActualChildren();

				return _actualChildren;
			}
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (!string.IsNullOrEmpty(NodePath))
			{
				Node = assetManager.LoadSceneNode(NodePath);
			}
		}

		private void UpdateActualChildren()
		{
			if (!_childrenDirty)
			{
				return;
			}

			_actualChildren.Clear();

			// Add prefab first
			if (_node != null)
			{
				_actualChildren.Add(_node);
			}

			// Then other children
			_actualChildren.AddRange(Children);

			_childrenDirty = false;
		}

		protected override void OnChildrenChanged()
		{
			base.OnChildrenChanged();

			_childrenDirty = true;
		}

		protected override SceneNode CreateInstanceCore() => new SubsceneNode();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var subscene = (SubsceneNode)node;
			Node = subscene.Node.Clone();
			NodePath = subscene.NodePath;
		}
	}
}
