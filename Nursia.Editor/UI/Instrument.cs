using Microsoft.Xna.Framework;
using Nursia.SceneGraph.Landscape;
using Nursia.Utility;
using System;

namespace Nursia.Editor.UI
{
	public enum TerrainInstrumentType
	{
		RaiseTerrain,
		LowerTerrain,
		PaintTexture1,
		PaintTexture2,
		PaintTexture3,
		PaintTexture4,
	}

	public class TerrainInstrument
	{
		private TerrainNode _terrain;
		private Color[] _splatData;
		private bool _splatDirty = false;

		public TerrainNode Terrain
		{
			get => _terrain;

			set
			{
				_terrain = value;
				_splatData = null;

				if (_terrain != null)
				{
					MinHeight = -_terrain.TerrainSize.Y;
					MaxHeight = _terrain.TerrainSize.Y;

					var material = _terrain.Material as TerrainMaterial;
					if (material != null && material.WeightMap1 != null)
					{
						SplatWidth = material.WeightMap1.Width;
						SplatHeight = material.WeightMap1.Height;
						_splatData = new Color[SplatWidth * SplatHeight];
						material.WeightMap1.GetData(_splatData);
						_splatDirty = false;
					}
				}
			}
		}

		public int SplatWidth { get; private set; }
		public int SplatHeight { get; private set; }

		public TerrainInstrumentType Type { get; set; }
		public float Radius { get; set; } = 5.0f;
		public float Power { get; set; } = 0.1f;
		public float MinHeight { get; set; }
		public float MaxHeight { get; set; }

		public Color GetSplatData(int x, int z)
		{
			x = MathUtils.Clamp(x, 0, SplatWidth);
			z = MathUtils.Clamp(z, 0, SplatHeight);

			return _splatData[x + z * SplatWidth];
		}

		public void SetSplatData(int x, int z, Color c)
		{
			x = MathUtils.Clamp(x, 0, SplatWidth);
			z = MathUtils.Clamp(z, 0, SplatHeight);

			var pos = x + z * SplatWidth;
			if (_splatData[pos] != c)
			{
				_splatData[pos] = c;
				_splatDirty = true;
			}
		}

		public void UpdateSplatTexture()
		{
			if (!_splatDirty)
			{
				return;
			}

			var material = _terrain.Material as TerrainMaterial;
			if (material != null && material.WeightMap1 != null)
			{
				material.WeightMap1.SetData(_splatData);
			}

			_splatDirty = false;
		}
	}

	public static class TerrainInstrumentExtensions
	{
		public static int GetChannelIndex(this TerrainInstrumentType type)
		{
			switch (type)
			{
				case TerrainInstrumentType.PaintTexture1:
					return 0;
				case TerrainInstrumentType.PaintTexture2:
					return 1;
				case TerrainInstrumentType.PaintTexture3:
					return 2;
				case TerrainInstrumentType.PaintTexture4:
					return 3;
			}

			throw new Exception($"Instrument {type} can't be converted to channel index");
		}
	}
}
