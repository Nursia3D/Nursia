using AssetManagementBase;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// A scene node that contains another scene graph as a child.
	/// </summary>
	[EditorInfo("Base")]
	public class SubsceneNode : SceneNode
	{
		private SceneNode _node;

		/// <summary>
		/// Gets or sets the root node of the subscene.
		/// </summary>
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

				InvalidateActualChildren();

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

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (!string.IsNullOrEmpty(NodePath))
			{
				Node = assetManager.LoadSceneNode(NodePath);
			}
		}

		protected override SceneNode CreateInstanceCore() => new SubsceneNode();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var subscene = (SubsceneNode)node;
			Node = subscene.Node.Clone();
			NodePath = subscene.NodePath;
		}

		protected override SceneNode GetCustomChild() => Node;
	}
}
