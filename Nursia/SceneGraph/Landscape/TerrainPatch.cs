// This code had been borrowed from https://github.com/gameplay3d/gameplay
// And ported to C#

using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Data.Landscape;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nursia.SceneGraph.Landscape
{
	/// <summary>
	/// Defines a single patch for a Terrain.
	/// </summary>
	public partial class TerrainPatch : DrDisposable
	{
		private bool _dirty = true;
		private readonly List<DrMeshPart> _levels = new List<DrMeshPart>();
		private int _maxStepShift = 0;

		private Terrain Terrain { get; }

		public int X1 { get; }
		public int X2 { get; }
		public int Z1 { get; }
		public int Z2 { get; }
		public int MaxStep { get; }


		public List<DrMeshPart> Levels
		{
			get
			{
				Update();

				return _levels;
			}
		}

		public BoundingBox BoundingBox => Levels[0].BoundingBox;
		public DateTime InvalidateTime { get; private set; }

		public TerrainPatch(Terrain terrain,
							int x1, int z1, int x2, int z2,
							int maxStep)
		{
			Terrain = terrain;
			X1 = x1;
			X2 = x2;
			Z1 = z1;
			Z2 = z2;
			MaxStep = maxStep;
		}

		public override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			foreach (var level in _levels)
			{
				level.Dispose();
			}

			_levels.Clear();
		}

		internal bool PositionAffectsPatch(int x, int z)
		{
			return X1 - _maxStepShift <= x && x <= X2 + _maxStepShift && Z1 - _maxStepShift <= z && z <= Z2 + _maxStepShift;
		}

		internal void Invalidate()
		{
			_dirty = true;
			InvalidateTime = DateTime.Now;
		}

		internal void Update()
		{
			if (!_dirty)
			{
				return;
			}

			// Destroy previous lods
			foreach (var level in _levels)
			{
				level.Dispose();
			}

			_levels.Clear();

			// Create new ones
			_maxStepShift = 0;
			for (var step = 1; step <= MaxStep; step *= 2)
			{
				AddLOD(step);
				_maxStepShift = Math.Max(_maxStepShift, step);
			}

			_dirty = false;

			Debug.WriteLine($"Patch Updated: X={X1}-{X2}, Z={Z1}-{Z2}");
		}

		private void AddLOD(int step)
		{
			var width = Terrain.HeightField.Columns;
			var height = Terrain.HeightField.Rows;

			// Allocate vertex data for this patch
			int patchWidth;
			int patchHeight;
			if (step == 1)
			{
				patchWidth = X2 - X1 + 1;
				patchHeight = Z2 - Z1 + 1;
			}
			else
			{
				patchWidth = (X2 - X1) / step + ((X2 - X1) % step == 0 ? 0 : 1) + 1;
				patchHeight = (Z2 - Z1) / step + ((Z2 - Z1) % step == 0 ? 0 : 1) + 1;
			}

			if (patchWidth < 2 || patchHeight < 2)
				return; // ignore this level, not enough geometry

			if (Terrain.VerticalSkirtScale > 0.0f)
			{
				patchWidth += 2;
				patchHeight += 2;
			}

			int vertexCount = patchHeight * patchWidth;
			var meshBuilder = new MeshBuilder();
			int index = 0;

			bool zskirt = Terrain.VerticalSkirtScale > 0;
			for (int z = Z1; ;)
			{
				bool xskirt = Terrain.VerticalSkirtScale > 0;
				for (int x = X1; ;)
				{
					Debug.Assert(index < vertexCount);

					index++;

					// Compute position - apply the local scale of the terrain into the vertex data
					var p2 = Terrain.HeightFieldToWorld2D(x, z);

					var pos = new Vector3(p2.X, Terrain.ComputeHeight(x, z), p2.Y);

					if (xskirt || zskirt)
					{
						pos.Y -= Terrain.VerticalSkirtScale * Terrain._localScale.Y;
					}

					var v = new VertexPositionNormalTexture
					{
						Position = pos
					};

					// Compute normal
					var normal = Terrain.ComputeNormal(x, z, step);
					v.Normal = normal;

					// Compute texture coord
					var texCoord = new Vector2((float)x / (width - 1), 1.0f - (float)z / (height - 1));
					if (xskirt)
					{
						float offset = Terrain.VerticalSkirtScale / width;
						texCoord.X = x == X1 ? texCoord.X - offset : texCoord.X + offset;
					}
					else if (zskirt)
					{
						float offset = Terrain.VerticalSkirtScale / height;
						texCoord.Y = z == Z1 ? texCoord.Y - offset : texCoord.Y + offset;
					}

					v.TextureCoordinate = texCoord;
					meshBuilder.AddVertex(v);

					if (x == X2)
					{
						if (Terrain.VerticalSkirtScale == 0 || xskirt)
						{
							break;
						}
						else
						{
							xskirt = true;
						}
					}
					else if (xskirt)
					{
						xskirt = false;
					}
					else
					{
						x = Math.Min(x + step, X2);
					}
				}

				if (z == Z2)
				{
					if (Terrain.VerticalSkirtScale == 0 || zskirt)
					{
						break;
					}
					else
					{
						zskirt = true;
					}
				}
				else if (zskirt)
				{
					zskirt = false;
				}
				else
				{
					z = Math.Min(z + step, Z2);
				}
			}

			Debug.Assert(index == vertexCount);

			for (int z = 0; z < patchHeight - 1; ++z)
			{
				int top = z * patchWidth;
				int bottom = (z + 1) * patchWidth;

				for (int x = 0; x < patchWidth - 1; ++x)
				{
					var topLeft = top + x;
					var bottomLeft = bottom + x;
					var topRight = topLeft + 1;
					var bottomRight = bottomLeft + 1;

					meshBuilder.AddIndex(bottomLeft);
					meshBuilder.AddIndex(topLeft);
					meshBuilder.AddIndex(topRight);
					meshBuilder.AddIndex(bottomLeft);
					meshBuilder.AddIndex(topRight);
					meshBuilder.AddIndex(bottomRight);
				}
			}

			var meshPart = meshBuilder.CreateMeshPart(Nrs.GraphicsDevice);
			// meshPart.Tag = _levels.Count;
			// meshPart.Tag = InvalidateTime;
			_levels.Add(meshPart);
		}
	}
}
