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

		/// <summary>
		/// Gets or sets the asset path to load the subscene from.
		/// </summary>
		[Browsable(false)]
		public string NodePath { get; set; }

		/// <summary>
		/// Gets the bounding box of the subscene or its node.
		/// </summary>
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

		/// <summary>
		/// Loads the subscene node from the asset manager if NodePath is set.
		/// </summary>
		/// <param name="assetManager">The asset manager to load resources from.</param>
		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (!string.IsNullOrEmpty(NodePath))
			{
				Node = assetManager.LoadSceneNode(NodePath);
			}
		}

		/// <summary>
		/// Creates a new instance of the SubsceneNode class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new SubsceneNode();

		/// <summary>
		/// Copies all properties from another subscene node to this node.
		/// </summary>
		/// <param name="node">The source node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var subscene = (SubsceneNode)node;
			Node = subscene.Node.Clone();
			NodePath = subscene.NodePath;
		}

		/// <summary>
		/// Gets the custom child node for this subscene.
		/// </summary>
		protected override SceneNode GetCustomChild() => Node;
	}
}
