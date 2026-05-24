### Overview

The purpose of this tutorial is to introduce Nursia and its base functionality such as rendering, scenes and asset management.

### Steps

1. Create MonoGame/FNA project.
2. [Reference Nursia](docs/adding-reference-to-nursia.md)
3. Download [Assets.zip](~/files/Assets.zip). Unzip it into the project folder.
4. Add following lines into .csproj so assets would be copied into the build output folder:
```xml
  <ItemGroup>
    <None Update="Assets\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>  
```
5. Use following Game class:
```c#
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
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private Camera _camera;
		private SceneNode _root;
		private CameraInputController _controller;
		private FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private SpriteBatch _spriteBatch;

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

			// Create the perspective camera at position (0, 25, 25) looking at (0, 0, 0)
			_camera = new Camera
			{
				View = Matrix.CreateLookAt(new Vector3(0, 25, 25), Vector3.Zero, Vector3.Up)
			};

			// Create the camera controller that will allow to move and rotate using keyboard and mouse
			_controller = new CameraInputController(_camera);

			// Root scene node
			_root = new SceneNode();

			// Direct light
			var directLight = new DirectLight
			{
				Rotation = new Vector3(320, 320, 0),
				Color = Color.White,
			};
			_root.Children.Add(directLight);

			// Create the asset manager
			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));

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
			_root.Children.Add(plane);

			// Add model
			var model = assetManager.LoadModelNode("Models/mixamo_base.glb");
			model.Scale = new Vector3(5, 5, 5);
			_root.Children.Add(model);

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
			_root.Children.Add(sphere);

			var capsule = new Capsule
			{
				Material = new BlinnPhongMaterial
				{
					DiffuseColor = Color.RoyalBlue
				},
				Translation = new Vector3(15, 15, 0),
				Scale = new Vector3(5, 5, 5)
			};
			_root.Children.Add(capsule);

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

			_controller.Update();
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			// Render the scene
			_renderer.Render(_root, _camera);

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

```
5. Run the project:

![alt text](~/images/quick-start-tutorial.png)

6. You could move around the scene using following controls: [CameraInputController](docs/camera-input-controller.md)