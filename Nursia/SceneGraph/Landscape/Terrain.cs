// This code had been borrowed from https://github.com/gameplay3d/gameplay
// And ported to C#

using Microsoft.Xna.Framework;
using Nursia.SceneGraph.Landscape;
using Nursia.Utilities;
using System;

namespace Nursia.Data.Landscape
{
	public partial class Terrain
	{
		private enum DirtyType
		{
			None,
			All,
			SomePatches
		}

		private enum QuadTreeDirtyType
		{
			None,
			All,
			BoundingBoxes
		}


		private HeightField _heightfield;
		internal Vector3 _localScale;
		private BoundingBox? _boundingBox;
		private Vector3 _size = new Vector3(1000, 500, 1000);
		private DirtyType _dirty = DirtyType.All;
		private bool _boundingBoxDirty = true;
		private QuadTreeDirtyType _patchTreeDirty = QuadTreeDirtyType.All;
		private int _detailLevels = 3;
		private int _patchSize = 32;
		private float _verticalSkirtScale = 0.1f;
		private float _halfWidth, _halfHeight;
		private TerrainPatch[,] _patches;
		private QuadTree<TerrainPatch> _patchTree;

		public HeightField HeightField
		{
			get => _heightfield;

			set
			{
				if (value == _heightfield)
				{
					return;
				}

				if (_heightfield != null)
				{
					_heightfield.HeightChanged -= OnHeightChanged;
				}

				_heightfield = value;

				if (_heightfield != null)
				{
					_heightfield.HeightChanged += OnHeightChanged;
				}

				UpdateSizes();
				InvalidateAll();
			}
		}

		public Vector3 Size
		{
			get => _size;

			set
			{
				if (value.EpsilonEquals(_size))
				{
					return;
				}

				_size = value;

				UpdateSizes();
				InvalidateAll();
			}
		}

		public int DetailLevels
		{
			get => _detailLevels;

			set
			{
				if (value == _detailLevels)
				{
					return;
				}

				_detailLevels = value;
				InvalidateAll();
			}
		}

		public int PatchSize
		{
			get => _patchSize;

			set
			{
				if (value == _patchSize)
				{
					return;
				}

				_patchSize = value;
				InvalidateAll();
			}
		}

		public float VerticalSkirtScale
		{
			get => _verticalSkirtScale;

			set
			{
				if (value.EpsilonEquals(_verticalSkirtScale))
				{
					return;
				}

				_verticalSkirtScale = value;
				InvalidateAll();
			}
		}

		public TerrainPatch[,] Patches
		{
			get
			{
				Update();

				return _patches;
			}
		}

		public int PatchesColumns => Patches != null ? Patches.GetLength(0) : 0;
		public int PatchesRows => Patches != null ? Patches.GetLength(1) : 0;

		public BoundingBox? BoundingBox
		{
			get
			{
				Update();
				if (_boundingBoxDirty)
				{
					BoundingBox? bounds = null;
					for (var col = 0; col < PatchesColumns; ++col)
					{
						for (var row = 0; row < PatchesRows; ++row)
						{
							var patch = _patches[col, row];
							if (bounds == null)
							{
								bounds = patch.BoundingBox;
							}
							else
							{
								bounds = Microsoft.Xna.Framework.BoundingBox.CreateMerged(bounds.Value, patch.BoundingBox);
							}
						}
					}

					_boundingBox = bounds;
					_boundingBoxDirty = false;
				}

				return _boundingBox;
			}
		}

		private DirtyType Dirty
		{
			get => _dirty;

			set
			{
				if (value == _dirty)
				{
					return;
				}

				_dirty = value;
				Invalid?.Invoke(this, EventArgs.Empty);
			}

		}

		public event EventHandler Invalid;

		public Terrain()
		{
		}

		public float GetHeight(Vector2 worldPos)
		{
			var hfPos = WorldToHeightField(worldPos);

			return HeightField.CalculateInterpolatedHeight(hfPos.X, hfPos.Y) * _localScale.Y;
		}

		public Vector3 ComputeNormal(Vector2 worldPos)
		{
			var hfPos = WorldToHeightField(worldPos);

			return ComputeNormal((int)hfPos.X, (int)hfPos.Y, 1);
		}

		internal float ComputeHeight(int x, int z)
		{
			return HeightField.GetHeight(x, z) * _localScale.Y;
		}


		internal Vector3 ComputeNormal(int x, int z, int step)
		{
			var width = HeightField.Columns;
			var height = HeightField.Rows;

			var p2 = HeightFieldToWorld2D(x, z);

			var p = new Vector3(p2.X, ComputeHeight(x, z), p2.Y);

			var stepXScaled = step * _localScale.X;
			var stepZScaled = step * _localScale.Z;

			var w = p - new Vector3(x >= step ? p2.X - stepXScaled : p2.X, ComputeHeight(x >= step ? x - step : x, z), p2.Y);
			var e = p - new Vector3(x < width - step ? p2.X + stepXScaled : p2.X, ComputeHeight(x < width - step ? x + step : x, z), p2.Y);
			var s = p - new Vector3(p2.X, ComputeHeight(x, z >= step ? z - step : z), z >= step ? p2.Y - stepZScaled : p2.Y);
			var n = p - new Vector3(p2.X, ComputeHeight(x, z < height - step ? z + step : z), z < height - step ? p2.Y + stepZScaled : p2.Y);

			var n0 = Vector3.Cross(n, w);
			var n1 = Vector3.Cross(w, s);
			var n2 = Vector3.Cross(e, n);
			var n3 = Vector3.Cross(s, e);
			Vector3 normal = -(n0 + n1 + n2 + n3);

			normal.Normalize();

			return normal;
		}

		internal Vector3 ComputeNormal(int x, int z) => ComputeNormal(x, z, 1);

		public Vector2 WorldToHeightField(Vector2 pos)
		{
			var result = new Vector2((pos.X / _localScale.X) + _halfWidth, (pos.Y / _localScale.Z) + _halfHeight);

			result.X = MathHelper.Clamp(result.X, 0, HeightField.Columns - 1);
			result.Y = MathHelper.Clamp(result.Y, 0, HeightField.Rows - 1);

			return result;
		}

		internal Vector2 HeightFieldToWorld2D(float x, float z)
		{
			return new Vector2((x - _halfWidth) * _localScale.X, (z - _halfHeight) * _localScale.Z);
		}


		public Vector3 HeightFieldToWorld(float x, float z)
		{
			var pos = HeightFieldToWorld2D(x, z);
			var height = HeightField.CalculateInterpolatedHeight(x, z) * _localScale.Y;

			return new Vector3(pos.X, height, pos.Y);
		}

		public Vector2 WorldToWeightMapNormalized(Vector2 pos)
		{
			var result = new Vector2(((pos.X / _localScale.X) + _halfWidth) / HeightField.Columns, 1.0f - ((pos.Y / _localScale.Z) + _halfHeight) / HeightField.Rows);

			result.X = MathHelper.Clamp(result.X, 0.0f, 1.0f);
			result.Y = MathHelper.Clamp(result.Y, 0.0f, 1.0f);

			return result;
		}

		public Vector2 WeightMapNormalizedToWorld(float x, float z)
		{
			Update();
			return new Vector2((x * HeightField.Columns - _halfWidth) * _localScale.X, ((1.0f - z) * HeightField.Rows - _halfHeight) * _localScale.Z);
		}

		private void OnHeightChanged(Point pos)
		{
			if (_patches == null)
			{
				return;
			}

			// We can't use PatchesColumns/PatchesRows here, since they call Update
			var cols = _patches.GetLength(0);
			var rows = _patches.GetLength(1);

			for (var col = 0; col < cols; ++col)
			{
				for (var row = 0; row < rows; ++row)
				{
					var patch = _patches[col, row];
					if (patch.PositionAffectsPatch(pos.X, pos.Y))
					{
						patch.Invalidate();
					}
				}
			}

			Dirty = DirtyType.SomePatches;
		}

		private void UpdateSizes()
		{
			if (HeightField == null)
			{
				return;
			}

			var width = HeightField.Columns;
			var height = HeightField.Rows;

			// Compute terrain scale
			_localScale = new Vector3(Size.X / (width - 1), Size.Y, Size.Z / (height - 1));

			_halfWidth = (width - 1) * 0.5f;
			_halfHeight = (height - 1) * 0.5f;
		}

		private void Update()
		{
			if (Dirty == DirtyType.None)
			{
				return;
			}

			if (Dirty == DirtyType.All)
			{
				// Rebuild everything
				if (HeightField != null)
				{
					UpdateSizes();

					var width = HeightField.Columns;
					var height = HeightField.Rows;

					var columns = width / PatchSize;
					var rows = height / PatchSize;
					_patches = new TerrainPatch[columns, rows];

					// Compute the maximum step size, which is a function of our lowest level of detail.
					// This determines how many vertices will be skipped per triange/quad on the lowest
					// level detail terrain patch.
					int maxStep = (int)Math.Pow(2.0, DetailLevels - 1);

					// Create terrain patches
					for (var row = 0; row < rows; ++row)
					{
						for (var col = 0; col < columns; ++col)
						{
							var x1 = col * PatchSize;
							var x2 = Math.Min(x1 + PatchSize, width - 1);

							var z1 = row * PatchSize;
							var z2 = Math.Min(z1 + PatchSize, height - 1);

							// Create this patch
							var patch = new TerrainPatch(this, x1, z1, x2, z2, maxStep);
							_patches[col, row] = patch;
						}
					}
				}

				InvalidatePatchTree(true);
				InvalidatePickTree(true);
			}
			else
			{
				// Just update patches
				var cols = _patches.GetLength(0);
				var rows = _patches.GetLength(1);

				for (var col = 0; col < cols; ++col)
				{
					for (var row = 0; row < rows; ++row)
					{
						var patch = _patches[col, row];
						patch.Update();
					}
				}

				InvalidatePatchTree(false);
				InvalidatePickTree(false);
			}

			InvalidateBoundingBox();

			Dirty = DirtyType.None;
		}

		private void UpdatePatchTree()
		{
			Update();

			if (_patchTreeDirty == QuadTreeDirtyType.None)
			{
				return;
			}

			if (HeightField != null)
			{
				if (_patchTreeDirty == QuadTreeDirtyType.All)
				{
					// Build patch tree
					_patchTree = new QuadTree<TerrainPatch>(new Rectangle(0, 0, PatchesColumns, PatchesRows),
						pos => _patches[pos.X, pos.Y],
						pos =>
						{
							var patch = _patches[pos.X, pos.Y];
							return patch.BoundingBox;
						});
				} else
				{
					// Recalculate bounding boxes
					_patchTree.RecalculateBoundingBoxes();
				}
			}

			_patchTreeDirty = QuadTreeDirtyType.None;
		}

		internal void PatchTreeRecursiveAction<P>(Func<QuadTreeNode<TerrainPatch>, P, bool> action, P param)
		{
			UpdatePatchTree();

			_patchTree?.TreeRecursiveAction(action, param);
		}

		private void InvalidateAll()
		{
			Dirty = DirtyType.All;
		}

		private void InvalidateBoundingBox()
		{
			_boundingBox = null;
			_boundingBoxDirty = true;
		}

		private void InvalidatePatchTree(bool all)
		{
			if (all)
			{
				_patchTree = null;
				_patchTreeDirty = QuadTreeDirtyType.All;
			}
			else
			{
				_patchTreeDirty = QuadTreeDirtyType.BoundingBoxes;
			}
		}
	}
}
