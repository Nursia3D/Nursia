using Microsoft.Xna.Framework;
using Nursia.Data.Landscape;
using Nursia.SceneGraph.Landscape;
using System.IO;
using Xunit;

namespace Nursia.Tests
{
	public sealed class TerrainTests
	{
		[Fact]
		public void TestLoadR16()
		{
			var assetManager = Utility.CreateAssetManager();

			HeightField heightField;
			using (var stream = assetManager.Open("heightMap.r16"))
			{
				heightField = HeightField.FromStreamR16(stream, 256, 256);
			}

			Assert.Equal(256, heightField.Columns);
			Assert.Equal(256, heightField.Rows);
			Assert.Equal(0.3343862f, heightField.CalculateInterpolatedHeight(0, 0), Utility.ZeroTolerance);
			Assert.Equal(0.106050201f, heightField.CalculateInterpolatedHeight(128, 0), Utility.ZeroTolerance);
		}

		[Fact]
		public void TestLoadSaveHF()
		{
			var assetManager = Utility.CreateAssetManager();

			// Load
			HeightField heightField;
			using (var stream = assetManager.Open("heightMap.hf"))
			{
				heightField = HeightField.FromStreamHf(stream);
			}

			// Save
			byte[] output;
			using (var ms = new MemoryStream())
			{
				heightField.SaveToHf(ms);
				output = ms.ToArray();
			}

			// Load saved
			HeightField heightField2;
			using (var stream = new MemoryStream(output))
			{
				heightField2 = HeightField.FromStreamHf(stream);
			}

			Assert.Equal(heightField.Columns, heightField2.Columns);
			Assert.Equal(heightField.Rows, heightField2.Columns);

			for (var x = 0; x < heightField.Columns; ++x)
			{
				for (var y = 0; y < heightField.Rows; ++y)
				{
					Assert.Equal(heightField.GetHeight(x, y), heightField2.GetHeight(x, y), Utility.ZeroTolerance);
				}
			}
		}

		[Fact]
		public void TestTerrain()
		{
			var assetManager = Utility.CreateAssetManager();

			var data = assetManager.ReadAsByteArray("heightmap.hf");
			var heightField = HeightField.FromHfBytes(data);

			var terrain = new Terrain
			{
				HeightField = heightField,
				Size = new Vector3(10000, 4000, 10000),
				DetailLevels = 3,
				VerticalSkirtScale = 0.1f
			};

			Utility.AssertAreEqual(new BoundingBox(new Vector3(-5000f, -125.337585f, -5000f), new Vector3(5000f, 2948.5315f, 5000f)), terrain.BoundingBox.Value);
			Assert.Equal(8, terrain.PatchesColumns);
			Assert.Equal(8, terrain.PatchesRows);

			TerrainPatch patch;
			for (var row = 0; row < terrain.PatchesRows; ++row)
			{
				for (var col = 0; col < terrain.PatchesColumns; ++col)
				{
					patch = terrain.Patches[row, col];
					Assert.Equal(3, patch.Levels.Count);
					Assert.Equal(361, patch.Levels[1].NumVertices);
					Assert.Equal(121, patch.Levels[2].NumVertices);
				}
			}

			patch = terrain.Patches[0, 0];
			Assert.Equal(1225, patch.Levels[0].NumVertices);

			patch = terrain.Patches[7, 0];
			Assert.Equal(1190, patch.Levels[0].NumVertices);
		}
	}
}
