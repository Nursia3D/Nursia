using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using Nursia.SceneGraph.Primitives;
using Nursia.SceneGraph.Water;
using Nursia.Utilities;
using Xunit;
using Plane = Nursia.SceneGraph.Primitives.Plane;

namespace Nursia.Tests
{
	public sealed class SceneTests
	{
		private static void TestShip(NursiaModelNode ship)
		{
			Assert.Equal("../Models/ship1.glb", ship.ModelPath);
			Assert.NotNull(ship.Model);

			Assert.Single(ship.Model.Meshes);
			Assert.Single(ship.Model.Meshes[0].MeshParts);

			Assert.Single(ship.Materials);
			Assert.Single(ship.Materials[0]);

			Assert.IsAssignableFrom<BlinnPhongMaterial>(ship.Materials[0][0]);
			var BlinnPhongMaterial = (BlinnPhongMaterial)ship.Materials[0][0];
		}

		[Fact]
		public void TestScene()
		{
			var assetManager = Utility.CreateAssetManager();

			var stored = assetManager.LoadStoredScene("Scenes/Test.scene");
			var env = stored.RenderEnvironment;
			Assert.NotNull(env);

			Assert.True(env.FogEnabled);
			Utility.AssertAreEqual(200.0f, env.FogStart);
			Assert.Equal(ColorStorage.FromName("#0379FFFF"), env.FogColor);

			var root = stored.Root;
			Assert.NotNull(root);
			Assert.Equal(7, root.Children.Count);

			var light = root.QueryFirstByType<DirectLight>();
			Assert.NotNull(light);
			Utility.AssertAreEqual(new Vector3(0, 100.0f, 0), light.Translation);
			Utility.AssertAreEqual(new Vector3(320, 90, 0), light.Rotation);

			var plane = (Plane)root.QueryFirstById("_plane");
			Assert.NotNull(plane);
			Utility.AssertAreEqual(new Vector2(512, 512), plane.Size);
			Utility.AssertAreEqual(32.0f, plane.UScale);
			Utility.AssertAreEqual(32.0f, plane.VScale);

			Assert.IsAssignableFrom<BlinnPhongMaterial>(plane.Material);
			var BlinnPhongMaterial = (BlinnPhongMaterial)plane.Material;
			Assert.Equal("../Textures/checker.dds", BlinnPhongMaterial.DiffuseTexturePath);
			Assert.NotNull(BlinnPhongMaterial.DiffuseTexture);
			Assert.Equal(128, BlinnPhongMaterial.DiffuseTexture.Width);
			Assert.Equal(128, BlinnPhongMaterial.DiffuseTexture.Height);

			var capsule = root.QueryFirstByType<Capsule>();
			Assert.NotNull(capsule);

			Assert.IsAssignableFrom<UnlitMaterial>(capsule.Material);
			var unlitMaterial = (UnlitMaterial)capsule.Material;
			Assert.True(unlitMaterial.IsTransparent);
			Assert.Equal(ColorStorage.FromName("#4CD96151"), unlitMaterial.DiffuseColor);

			var sphere = (Sphere)root.QueryFirstById("_sphere");
			Assert.NotNull(sphere);
			Utility.AssertAreEqual(new Vector3(10, 10, 10), sphere.Scale);
			Assert.IsAssignableFrom<BlinnPhongMaterial>(sphere.Material);
			BlinnPhongMaterial = (BlinnPhongMaterial)sphere.Material;
			Assert.Equal(BlendState.Additive, BlinnPhongMaterial.BlendState);
			Assert.Equal(DepthStencilState.Default, BlinnPhongMaterial.DepthStencilState);
			Assert.Equal(RasterizerState.CullNone, BlinnPhongMaterial.RasterizerState);
			Assert.False(BlinnPhongMaterial.CastsShadows);

			var ship1 = (SubsceneNode)root.QueryFirstById("_ship1");
			Assert.NotNull(ship1);
			Utility.AssertAreEqual(new Vector3(0, 20, 50), ship1.Translation);
			Assert.Equal("Model.scene", ship1.NodePath);

			Assert.IsAssignableFrom<NursiaModelNode>(ship1.Node);
			TestShip((NursiaModelNode)ship1.Node);

			var water = root.QueryFirstByType<WaterNode>();
			Assert.NotNull(water);

			Assert.IsAssignableFrom<WaterMaterial>(water.Material);
			var waterMaterial = (WaterMaterial)water.Material;

			Assert.Equal(ColorStorage.FromName("#264AFFFF"), waterMaterial.WaterTint);
		}
	}
}
