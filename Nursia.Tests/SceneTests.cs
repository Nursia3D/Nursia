using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using Nursia.SceneGraph.Primitives;
using Nursia.SceneGraph.Water;
using Nursia.Utilities;

using Plane = Nursia.SceneGraph.Primitives.Plane;

namespace Nursia.Tests
{
	[TestClass]
	public sealed class SceneTests
	{
		private static void TestShip(NursiaModelNode ship)
		{
			Assert.AreEqual("../Models/ship1.glb", ship.ModelPath);
			Assert.IsNotNull(ship.Model);

			Assert.AreEqual(1, ship.Model.Meshes.Length);
			Assert.AreEqual(1, ship.Model.Meshes[0].MeshParts.Count);

			Assert.AreEqual(1, ship.Materials.Length);
			Assert.AreEqual(1, ship.Materials[0].Length);

			Assert.IsInstanceOfType<BlinnPhongMaterial>(ship.Materials[0][0]);
			var BlinnPhongMaterial = (BlinnPhongMaterial)ship.Materials[0][0];

			Assert.AreEqual("../Textures/ship1_c.tga", BlinnPhongMaterial.DiffuseTexturePath);
			Assert.AreEqual("../Textures/ship1_s.tga", BlinnPhongMaterial.SpecularTexturePath);
			Assert.AreEqual("../Textures/ship1_n.tga", BlinnPhongMaterial.NormalTexturePath);
			Assert.IsNotNull(BlinnPhongMaterial.DiffuseTexture);
			Assert.IsNotNull(BlinnPhongMaterial.SpecularTexture);
			Assert.IsNotNull(BlinnPhongMaterial.NormalTexture);
		}

		[TestMethod]
		public void TestSceme()
		{
			var assetManager = Utility.CreateAssetManager();

			var stored = assetManager.LoadStoredScene("Scenes/Test.scene");
			var env = stored.RenderEnvironment;
			Assert.IsNotNull(env);

			Assert.AreEqual(true, env.FogEnabled);
			Utility.AssertAreEqual(200.0f, env.FogStart);
			Assert.AreEqual(ColorStorage.FromName("#0379FFFF"), env.FogColor);

			var root = stored.Root;
			Assert.IsNotNull(root);
			Assert.AreEqual(7, root.Children.Count);

			var light = root.QueryFirstByType<DirectLight>();
			Assert.IsNotNull(light);
			Utility.AssertAreEqual(new Vector3(0, 100.0f, 0), light.Translation);
			Utility.AssertAreEqual(new Vector3(320, 90, 0), light.Rotation);

			var plane = (Plane)root.QueryFirstById("_plane");
			Assert.IsNotNull(plane);
			Utility.AssertAreEqual(new Vector2(512, 512), plane.Size);
			Utility.AssertAreEqual(32.0f, plane.UScale);
			Utility.AssertAreEqual(32.0f, plane.VScale);

			Assert.IsInstanceOfType<BlinnPhongMaterial>(plane.Material);
			var BlinnPhongMaterial = (BlinnPhongMaterial)plane.Material;
			Assert.AreEqual("../Textures/checker.dds", BlinnPhongMaterial.DiffuseTexturePath);
			Assert.IsNotNull(BlinnPhongMaterial.DiffuseTexture);
			Assert.AreEqual(128, BlinnPhongMaterial.DiffuseTexture.Width);
			Assert.AreEqual(128, BlinnPhongMaterial.DiffuseTexture.Height);

			var capsule = root.QueryFirstByType<Capsule>();
			Assert.IsNotNull(capsule);

			Assert.IsInstanceOfType<UnlitMaterial>(capsule.Material);
			var unlitMaterial = (UnlitMaterial)capsule.Material;
			Assert.AreEqual(true, unlitMaterial.IsTransparent);
			Assert.AreEqual(ColorStorage.FromName("#4CD96151"), unlitMaterial.DiffuseColor);

			var sphere = (Sphere)root.QueryFirstById("_sphere");
			Assert.IsNotNull(sphere);
			Utility.AssertAreEqual(new Vector3(10, 10, 10), sphere.Scale);
			Assert.IsInstanceOfType<BlinnPhongMaterial>(sphere.Material);
			BlinnPhongMaterial = (BlinnPhongMaterial)sphere.Material;
			Assert.AreEqual(BlendState.Additive, BlinnPhongMaterial.BlendState);
			Assert.AreEqual(DepthStencilState.Default, BlinnPhongMaterial.DepthStencilState);
			Assert.AreEqual(RasterizerState.CullNone, BlinnPhongMaterial.RasterizerState);
			Assert.AreEqual(false, BlinnPhongMaterial.CastsShadows);

			var ship1 = (SubsceneNode)root.QueryFirstById("_ship1");
			Assert.IsNotNull(ship1);
			Utility.AssertAreEqual(new Vector3(0, 20, 50), ship1.Translation);
			Assert.AreEqual("Model.scene", ship1.NodePath);

			Assert.IsInstanceOfType<NursiaModelNode>(ship1.Node);
			TestShip((NursiaModelNode)ship1.Node);

			var water = root.QueryFirstByType<WaterNode>();
			Assert.IsNotNull(water);

			Assert.IsInstanceOfType<WaterMaterial>(water.Material);
			var waterMaterial = (WaterMaterial)water.Material;

			Assert.AreEqual(ColorStorage.FromName("#264AFFFF"), waterMaterial.WaterTint);
		}
	}
}
