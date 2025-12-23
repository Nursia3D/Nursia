using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.SceneGraph.Lights;
using Nursia.Materials;
using Nursia.SceneGraph.Primitives;
using Nursia.Rendering;
using Nursia.SceneGraph;
using Nursia.Utilities;
using System;
using System.IO;
using System.Reflection;

namespace Nursia.Samples.Primives
{
	public class TutorialGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private SceneRenderer _renderer;
		private Scene _scene;
		private CameraInputController _controller;
		private FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private SpriteBatch _spriteBatch;

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public TutorialGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// Output asset manager log to console
			AMBConfiguration.Logger = Console.WriteLine;

			// Required to work with Nursia
			Nrs.Game = this;

			// Scene
			_scene = new Scene();
			
			// Root scene node
			var root = new SceneNode();

			// Direct light
			var directLight = new DirectLight
			{
				Rotation = new Vector3(320, 320, 0),
				Color = Color.White,
			};
			root.Children.Add(directLight);

			// Set scene root
			_scene.Root = root;

			// Create the perspective camera at position (0, 25, 25) looking at (0, 0, 0)
			var camera = new Camera
			{
				View = Matrix.CreateLookAt(new Vector3(0, 25, 25), Vector3.Zero, Vector3.Up)
			};

			// Create the camera controller that will allow to move and rotate using keyboard and mouse
			_controller = new CameraInputController(camera);

			// Set scene camera
			_scene.Camera = camera;

			// Create the asset manager
			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(ExecutingAssemblyDirectory, "Assets"));

			// Textured plane
			var plane = new SceneGraph.Primitives.Plane
			{
				NormalDirection = DigitalRiseModel.Primitives.NormalDirection.UpY,
				Scale = new Vector3(256, 1, 256),
				UScale = 50.0f,
				VScale = 50.0f,
				Material = new BlinnPhongMaterial
				{
					DiffuseColor = Color.White,
					DiffuseTexture = assetManager.LoadTexture2D(GraphicsDevice, "Textures/checker.dds"),
					CastsShadows = false
				}
			};
			root.Children.Add(plane);

			// Add model
			var model = assetManager.LoadModelNode("Models/mixamo_base.glb");
			model.Scale = new Vector3(5, 5, 5);
			root.Children.Add(model);

			// Add some more primitives
			var sphere = new Sphere
			{
				Material = new BlinnPhongMaterial
				{
					DiffuseColor = Color.Yellow
				},
				Translation = new Vector3(0, 15, -10),
				Scale = new Vector3(5, 5, 5)
			};
			root.Children.Add(sphere);

			var capsule = new Capsule
			{
				Material = new BlinnPhongMaterial
				{
					DiffuseColor = Color.RoyalBlue
				},
				Translation = new Vector3(15, 15, 0),
				Scale = new Vector3(5, 5, 5)
			};
			root.Children.Add(capsule);

			_renderer = new SceneRenderer(_scene);

			// Shadows settings
			Nrs.GraphicsSettings.ShadowMapSize = Shadows.ShadowMapSize.Size4096;
			Nrs.GraphicsSettings.MaxShadowDistance = 200.0f;
			Nrs.GraphicsSettings.ShadowType = ShadowType.PCF;

			// SpriteBatch
			_spriteBatch = new SpriteBatch(GraphicsDevice);
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
			_renderer.Render();

			_spriteBatch.Begin();

			var font = Nrs.DebugFont;
			_spriteBatch.DrawString(font, $"FPS: {_fpsCounter.FramesPerSecond}", new Vector2(0, 0), Color.White);
			_spriteBatch.DrawString(font, $"Effect Switches: {_renderer.Statistics.EffectsSwitches}", new Vector2(0, 24), Color.White);
			_spriteBatch.DrawString(font, $"Draw Calls: {_renderer.Statistics.DrawCalls}", new Vector2(0, 48), Color.White);
			_spriteBatch.DrawString(font, $"Vertices Drawn: {_renderer.Statistics.VerticesDrawn}", new Vector2(0, 72), Color.White);
			_spriteBatch.DrawString(font, $"Primitives Drawn: {_renderer.Statistics.PrimitivesDrawn}", new Vector2(0, 96), Color.White);

			_spriteBatch.End();

			_fpsCounter.OnFrameDrawn();
		}
	}
}
