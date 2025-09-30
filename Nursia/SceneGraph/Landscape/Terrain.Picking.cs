using Microsoft.Xna.Framework;
using Nursia.Rendering;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Landscape;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Data.Landscape
{
	partial class Terrain
	{
		private class PickTreeContext
		{
			public Matrix Transform;
			public Ray Ray;
			public readonly List<TerrainPatch> Patches = new List<TerrainPatch>();
			public readonly List<Vector3> Results = new List<Vector3>();

			public void Reset()
			{
				Patches.Clear();
				Results.Clear();
			}
		}

		private readonly PickTreeContext _pickTreeContext = new PickTreeContext();
		private QuadTree<bool> _pickTree;
		private QuadTreeDirtyType _pickTreeDirty = QuadTreeDirtyType.All;

		private void UpdatePickTree()
		{
			Update();

			if (_pickTreeDirty == QuadTreeDirtyType.None)
			{
				return;
			}

			if (HeightField != null)
			{
				if (_pickTreeDirty == QuadTreeDirtyType.All)
				{
					var area = new Rectangle(0, 0, HeightField.Columns, HeightField.Rows);
					_pickTree = new QuadTree<bool>(area,
						pos => false,
						pos =>
						{
							var points = new Vector3[]
							{
							HeightFieldToWorld(pos.X, pos.Y),
							HeightFieldToWorld(pos.X + 1, pos.Y),
							HeightFieldToWorld(pos.X, pos.Y + 1),
							HeightFieldToWorld(pos.X + 1, pos.Y + 1)
							};

							return Microsoft.Xna.Framework.BoundingBox.CreateFromPoints(points);
						});
				}
				else
				{
					_pickTree.RecalculateBoundingBoxes();
				}
			}

			_pickTreeDirty = QuadTreeDirtyType.None;
		}

		private static readonly Func<QuadTreeNode<bool>, PickTreeContext, bool> _pickTreeFindResults = (node, context) =>
		{
			var bb = node.BoundingBox.Transform(ref context.Transform);
			var dist = context.Ray.Intersects(bb);
			if (dist == null)
			{
				return false;
			}

			if (node.IsLeaf)
			{
				// Found
				context.Results.Add(context.Ray.Position + context.Ray.Direction * dist.Value);
				node.Data = true;
			}

			return true;
		};

		public Vector3? FindPosition(Matrix transform, Ray ray)
		{
			UpdatePickTree();

			if (_pickTree == null)
			{
				return null;
			}

			// Erase picked status
			_pickTree.TreeRecursiveAction(n =>
			{
				n.Data = false;
				return true;
			});

			// Prepare context
			_pickTreeContext.Ray = ray;
			_pickTreeContext.Transform = transform;
			_pickTreeContext.Reset();

			// Find intersections
			_pickTree.TreeRecursiveAction(_pickTreeFindResults, _pickTreeContext);

			if (_pickTreeContext.Results.Count == 0)
			{
				_pickTreeContext.Reset();
				return null;
			}

			// Choose the closest intersection
			var idx = 0;
			var lastDist = float.MaxValue;
			for (var i = 0; i < _pickTreeContext.Results.Count; ++i)
			{
				var dist = Vector3.DistanceSquared(ray.Position, _pickTreeContext.Results[i]);
				if (dist < lastDist)
				{
					idx = i;
					lastDist = dist;
				}
			}

			var result = _pickTreeContext.Results[idx];
			_pickTreeContext.Reset();

			return result;
		}

		internal void DebugDrawBoundingBoxes(Matrix transform, Camera camera)
		{
			if (_pickTree == null)
			{
				return;
			}

			_pickTree.TreeRecursiveAction(n =>
			{
				var bb = n.BoundingBox.Transform(ref transform);
				if (!camera.Frustum.Intersects(bb))
				{
					return false;
				}

				if (n.IsLeaf)
				{
					DebugShapeRenderer.AddBoundingBox(bb, n.Data ? Color.IndianRed : Color.LightGreen);
				}

				return true;
			});
		}

		private void InvalidatePickTree(bool all)
		{
			if (all)
			{
				_pickTree = null;
				_pickTreeDirty = QuadTreeDirtyType.All;
			}
			else
			{
				_pickTreeDirty = QuadTreeDirtyType.BoundingBoxes;
			}
		}
	}
}
