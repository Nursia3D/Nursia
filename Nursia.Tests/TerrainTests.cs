using Microsoft.Xna.Framework;
using Nursia.SceneGraph;
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
			using (var stream = assetManager.Open("heightmap.r16"))
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
			using (var stream = assetManager.Open("heightmap.hf"))
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

			var terrain = new TerrainNode
			{
				HeightField = heightField,
				TerrainSize = new Vector3(10000, 4000, 10000),
				DetailLevels = 3,
				VerticalSkirtScale = 0.1f
			};

			Utility.AssertAreEqual(new BoundingBox(new Vector3(-5000f, -125.337585f, -5000f), new Vector3(5000f, 2948.5315f, 5000f)), terrain.BoundingBox.Value);
			Assert.Equal(64, terrain.Patches.Count);

			MeshNode level;
			foreach (var patch in terrain.Patches)
			{
				Assert.Equal(3, patch.Children.Count);

				level = (MeshNode)patch.Children[1];
				Assert.Equal(361, level.Mesh.NumVertices);

				level = (MeshNode)patch.Children[2];
				Assert.Equal(121, level.Mesh.NumVertices);
			}

			level = (MeshNode)terrain.Patches[0].Children[0];

			Assert.Equal(1225, level.Mesh.NumVertices);
		}
	}
}
