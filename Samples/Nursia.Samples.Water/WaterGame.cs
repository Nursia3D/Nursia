using AssetManagementBase;
using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Landscape;
using Nursia.SceneGraph.Lights;
using Nursia.Utilities;
using System;
using System.IO;
using System.Reflection;

namespace Nursia.Samples.Primives
{
	public class WaterGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private Scene _scene;
		private CameraInputController _controller;
		private SpriteBatch _spriteBatch;

		public WaterGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1600,
				PreferredBackBufferHeight = 900,
				GraphicsProfile = GraphicsProfile.HiDef,
				SynchronizeWithVerticalRetrace = false
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;
			IsFixedTimeStep = false;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// Output asset manager log to console
			AMBConfiguration.Logger = Console.WriteLine;

			// Required to work with Nursia
			Nrs.Game = this;

			// Root scene node
			var root = new SceneNode();

			// Create the asset manager
			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));
			_scene = assetManager.LoadStoredScene("Scenes/Main.scene");

			root.Children.Add(_scene.Root);

			var terrain = root.QueryFirstByType<TerrainNode>();

			var checker = assetManager.LoadTexture2D(GraphicsDevice, "Textures/checker.dds");
			var material = new BlinnPhongMaterial
			{
				DiffuseTexture = checker
			};

			// Add some random boxes
			var rnd = new Random();

			var multiMesh = new InstancedMeshNode
			{
				Mesh = MeshPrimitives.CreateBoxMeshPart(GraphicsDevice, Vector3.One),
				Material = material
			};
			root.Children.Add(multiMesh);

			for (var i = 0; i < 1000; ++i)
			{
				var pos = new Vector3(rnd.Next(2000) - 1000, 0, rnd.Next(2000) - 1000);
				pos.Y = terrain.GetHeight(pos) + 2.25f;

				var normal = terrain.ComputeNormal(new Vector2(pos.X, pos.Z));
				var rotation = Utility.MakeRotationFromTo(Vector3.Up, normal);

				multiMesh.InstancesTransforms.Add(SrtTransform.CreateMatrix(pos, new Vector3(5.0f), rotation));
			}

			// Set new root
			_scene.Root = root;

			_controller = new CameraInputController(_scene.Camera);

			// SpriteBatch
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// DebugSettings.DrawBoundingBoxes = true;
			// Nrs.DebugSettings.VisualizeBuffer = DebugVisualizeBuffer.DepthBuffer;

			var light = root.QueryFirstByType<DirectLight>();
			// light.ShadowBias = 0.00002f;

			// Nrs.GraphicsSettings.ShadowMapSize = ShadowMapSize.Size8192;
			// Nrs.GraphicsSettings.ShadowType = ShadowType.None;
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_scene.Update(gameTime);
			_controller.Update();
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			// Render the scene
			_scene.Render(_renderer);

			_spriteBatch.Begin();

			var font = Nrs.DebugFont;
			_spriteBatch.DrawString(font, $"FPS: {_fpsCounter.FramesPerSecond}", new Vector2(0, 0), Color.White);
			_spriteBatch.DrawString(font, $"Effect Switches: {_renderer.Statistics.EffectsSwitches}", new Vector2(0, 24), Color.White);
			_spriteBatch.DrawString(font, $"Draw Calls: {_renderer.Statistics.DrawCalls}", new Vector2(0, 48), Color.White);
			_spriteBatch.DrawString(font, $"Vertices Drawn: {_renderer.Statistics.VerticesDrawn}", new Vector2(0, 72), Color.White);
			_spriteBatch.DrawString(font, $"Primitives Drawn: {_renderer.Statistics.PrimitivesDrawn}", new Vector2(0, 96), Color.White);
			_spriteBatch.DrawString(font, $"Passes: {_renderer.Statistics.Passes}", new Vector2(0, 120), Color.White);

			var camera = _scene.Camera;
			_spriteBatch.DrawString(font, $"Camera: {camera.Translation.X}, {camera.Translation.Y}, {camera.Translation.Z}", new Vector2(0, 144), Color.White);

			_spriteBatch.End();

			_fpsCounter.OnFrameDrawn();
		}
	}
}
