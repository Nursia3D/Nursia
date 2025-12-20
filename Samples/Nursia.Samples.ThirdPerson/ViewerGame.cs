using AssetManagementBase;
using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia.Rendering;
using Nursia.SceneGraph;
using StbImageSharp;
using System;
using System.IO;
using System.Reflection;

namespace Nursia.Samples.ThirdPerson
{
	public class ViewerGame : Game
	{
		private const float MouseSensitivity = 0.2f;
		private const float MovementSpeed = 0.05f;

		private readonly GraphicsDeviceManager _graphics;
		private Scene _scene;
		private SceneNode _cameraMount;
		private Camera _mainCamera;
		private NursiaModelNode _model;
		private AnimationController _player;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private SpriteBatch _spriteBatch;
		private InputService _inputService;

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

		public ViewerGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = false;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		private static void LoadCubeFace(TextureCube texture, CubeMapFace face, AssetManager assetManager, string name)
		{
			ImageResult image;
			using (var stream = assetManager.Open("Textures/" + name))
			{
				image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
			}

			texture.SetData(face, image.Data);
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// Nursia
			Nrs.Game = this;

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(ExecutingAssemblyDirectory, "Assets"));
			_scene = assetManager.LoadStoredScene("Scenes/Main.scene");

			_model = _scene.Root.QueryFirstByType<NursiaModelNode>();
			_player = new AnimationController(_model.ModelInstance);
			_player.StartClip("idle");

			_cameraMount = _scene.Root.QueryFirstById("_cameraMount");
			_mainCamera = _scene.Root.QueryFirstByType<Camera>();

			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;

			// Add reflection plane
			var reflectionPlane = new ReflectionPlane
			{
				Scale = new Vector3(64, 64, 1),
				Translation = new Vector3(-16, 0, 32),
				Rotation = new Vector3(0, 135, 0),
				DiffuseColor = Color.LightBlue
			};
			_scene.Root.Children.Add(reflectionPlane);

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// Nrs.DebugSettings.VisualizeBuffer = DebugVisualizeBuffer.ReflectionShadowMap;
			Nrs.GraphicsSettings.ShadowType = ShadowType.PCF;
			// Nrs.DebugSettings.DrawCamerasFrustums = true;
		}

		private void _inputService_MouseMoved(object sender, InputEventArgs<Point> e)
		{
			var playerRotation = _model.Rotation;
			playerRotation.Y += -(int)((e.NewValue.X - e.OldValue.X) * MouseSensitivity);
			_model.Rotation = playerRotation;

			var cameraRotation = _cameraMount.Rotation;
			cameraRotation.X += (int)((e.NewValue.Y - e.OldValue.Y) * MouseSensitivity);
			_cameraMount.Rotation = cameraRotation;
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_inputService.Update();

			var movement = 0;
			if (_inputService.IsKeyDown(Keys.W))
			{
				movement = -1;
			}
			else if (_inputService.IsKeyDown(Keys.S))
			{
				movement = 1;
			}

			if (_inputService.IsKeyDown(Keys.LeftShift) || _inputService.IsKeyDown(Keys.RightShift))
			{
				movement *= 2;
			}

			// Set animation
			switch (movement)
			{
				case 0:
					if (_player.AnimationClip.Name != "idle")
					{
						_player.StartClip("idle");
					}
					break;

				case 1:
				case -1:
					if (_player.AnimationClip.Name != "walking")
					{
						_player.StartClip("walking");
					}
					break;

				case 2:
				case -2:
					if (_player.AnimationClip.Name != "running")
					{
						_player.StartClip("running");
					}
					break;
			}

			// Perform the movement
			var velocity = _model.GlobalTransform.Forward * movement * MovementSpeed;
			_model.Translation += velocity;

			_player.Update(gameTime.ElapsedGameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			// Render the scene
			_renderer.Render(_scene.Root, _mainCamera, _scene.RenderEnvironment);

			_spriteBatch.Begin();

			var font = Nrs.DebugFont;
			_spriteBatch.DrawString(font, $"FPS: {_fpsCounter.FramesPerSecond}", new Vector2(0, 0), Color.White);
			_spriteBatch.DrawString(font, $"Effect Switches: {_renderer.Statistics.EffectsSwitches}", new Vector2(0, 24), Color.White);
			_spriteBatch.DrawString(font, $"Draw Calls: {_renderer.Statistics.DrawCalls}", new Vector2(0, 48), Color.White);
			_spriteBatch.DrawString(font, $"Vertices Drawn: {_renderer.Statistics.VerticesDrawn}", new Vector2(0, 72), Color.White);
			_spriteBatch.DrawString(font, $"Primitives Drawn: {_renderer.Statistics.PrimitivesDrawn}", new Vector2(0, 96), Color.White);
			_spriteBatch.DrawString(font, $"Passes: {_renderer.Statistics.Passes}", new Vector2(0, 120), Color.White);

			/*			_spriteBatch.Draw(_light.ShadowMap, 
							new Rectangle(0, 0, 256, 256), 
							Color.White);*/

			_spriteBatch.End();

			_fpsCounter.OnFrameDrawn();
		}
	}
}
