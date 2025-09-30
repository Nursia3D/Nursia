using Microsoft.Xna.Framework;
using System;

namespace Nursia.Utilities
{
	internal class QuadTreeNode<T>
	{
		public BoundingBox BoundingBox;
		public Rectangle Area;
		public QuadTreeNode<T> TopLeft, TopRight, BottomLeft, BottomRight;
		public bool IsLeaf => TopLeft == null;
		public T Data;
	}

	internal class QuadTree<T>
	{
		private readonly QuadTreeNode<T> _root;
		private readonly Func<Point, BoundingBox> _boundingBoxGetter;
		private int _nodesCount = 0;

		public BoundingBox BundingBox => _root.BoundingBox;
		public int NodesCount => _nodesCount;

		public QuadTree(Rectangle area, Func<Point, T> leafGetter, Func<Point, BoundingBox> boundingBoxGetter)
		{
			_boundingBoxGetter = boundingBoxGetter ?? throw new ArgumentNullException(nameof(boundingBoxGetter));
			_root = BuildTreeRecursively(area, leafGetter);
			RecalculateBoundingBoxes();
		}

		private void UpdateBoundingBoxes(QuadTreeNode<T> root)
		{
			if (!root.IsLeaf)
			{
				UpdateBoundingBoxes(root.TopLeft);
				UpdateBoundingBoxes(root.TopRight);
				UpdateBoundingBoxes(root.BottomLeft);
				UpdateBoundingBoxes(root.BottomRight);

				var bb = root.TopLeft.BoundingBox;
				bb = BoundingBox.CreateMerged(bb, root.TopRight.BoundingBox);
				bb = BoundingBox.CreateMerged(bb, root.BottomLeft.BoundingBox);
				bb = BoundingBox.CreateMerged(bb, root.BottomRight.BoundingBox);

				root.BoundingBox = bb;
			}
			else
			{
				root.BoundingBox = _boundingBoxGetter(new Point(root.Area.X, root.Area.Y));
			}
		}

		private QuadTreeNode<T> BuildTreeRecursively(Rectangle area, Func<Point, T> leafGetter)
		{
			var result = new QuadTreeNode<T>
			{
				Area = area
			};

			++_nodesCount;

			if (area.Width > 1 || area.Height > 1)
			{
				var halfWidth = area.Width / 2;
				var halfHeight = area.Height / 2;

				result.TopLeft = BuildTreeRecursively(new Rectangle(area.X, area.Y, halfWidth, halfHeight), leafGetter);
				result.TopRight = BuildTreeRecursively(new Rectangle(area.X + halfWidth, area.Y, halfWidth, halfHeight), leafGetter);
				result.BottomLeft = BuildTreeRecursively(new Rectangle(area.X, area.Y + halfHeight, halfWidth, halfHeight), leafGetter);
				result.BottomRight = BuildTreeRecursively(new Rectangle(area.X + halfWidth, area.Y + halfHeight, halfWidth, halfHeight), leafGetter);
			}
			else
			{
				// Leaf
				var data = leafGetter(new Point(area.X, area.Y));
				result.Data = data;
			}

			return result;
		}

		public void RecalculateBoundingBoxes()
		{
			UpdateBoundingBoxes(_root);
		}

		private void TreeRecursiveAction(QuadTreeNode<T> node, Func<QuadTreeNode<T>, bool> action)
		{
			if (!action(node))
			{
				return;
			}

			if (node.TopLeft != null)
			{
				TreeRecursiveAction(node.TopLeft, action);
				TreeRecursiveAction(node.TopRight, action);
				TreeRecursiveAction(node.BottomLeft, action);
				TreeRecursiveAction(node.BottomRight, action);
			}
		}

		public void TreeRecursiveAction(Func<QuadTreeNode<T>, bool> action) => TreeRecursiveAction(_root, action);

		private void TreeRecursiveAction<P>(QuadTreeNode<T> node, P param, Func<QuadTreeNode<T>, P, bool> action)
		{
			if (!action(node, param))
			{
				return;
			}

			if (node.TopLeft != null)
			{
				TreeRecursiveAction(node.TopLeft, param, action);
				TreeRecursiveAction(node.TopRight, param, action);
				TreeRecursiveAction(node.BottomLeft, param, action);
				TreeRecursiveAction(node.BottomRight, param, action);
			}
		}

		public void TreeRecursiveAction<P>(Func<QuadTreeNode<T>, P, bool> action, P param) => TreeRecursiveAction(_root, param, action);

		private void TreeRecursiveAction<P, P2>(QuadTreeNode<T> node, P param, P2 param2, Func<QuadTreeNode<T>, P, P2, bool> action)
		{
			if (!action(node, param, param2))
			{
				return;
			}

			if (node.TopLeft != null)
			{
				TreeRecursiveAction(node.TopLeft, param, param2, action);
				TreeRecursiveAction(node.TopRight, param, param2, action);
				TreeRecursiveAction(node.BottomLeft, param, param2, action);
				TreeRecursiveAction(node.BottomRight, param, param2, action);
			}
		}

		public void TreeRecursiveAction<P, P2>(Func<QuadTreeNode<T>, P, P2, bool> action, P param, P2 param2) => TreeRecursiveAction(_root, param, param2, action);
	}
}
