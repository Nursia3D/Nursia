using AssetManagementBase;
using Nursia.Samples.Character.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Nursia.Rendering;
using Nursia.SceneGraph;
using System;
using System.IO;

namespace Nursia.Samples.Character
{
	public class ViewerGame : Game
	{
		private const float MouseSensitivity = 0.2f;
		private const float MovementSpeed = 0.1f;

		private readonly GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private InputService _inputService;
		private Character _controllerService;
		private Scene _scene;
		private SceneNode _cameraMount = new SceneNode();
		private Camera _mainCamera = new Camera();
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private Desktop _desktop;
		private MainPanel _mainPanel;

		/// <summary>Singleton instance of the ViewerGame for global access.</summary>
		public static ViewerGame Instance { get; private set; }

		private NursiaModelNode ModelNode => _controllerService.ModelNode;

		/// <summary>Initializes game with graphics and input configuration.</summary>
		public ViewerGame()
		{
			Instance = this;

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			IsMouseVisible = true;
			Window.AllowUserResizing = true;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		/// <summary>Loads all game content and scene setup.</summary>
		protected override void LoadContent()
		{
			base.LoadContent();

			// Nursia
			Nrs.Game = this;

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));
			_scene = assetManager.LoadStoredScene("Scenes/Main.scene");

			// Add reflection plane
			var reflectionPlane = new ReflectionPlane
			{
				Scale = new Vector3(64, 64, 1),
				Translation = new Vector3(-16, 0, 32),
				Rotation = new Vector3(0, 135, 0),
				DiffuseColor = Color.LightBlue
			};
			_scene.Root.Children.Add(reflectionPlane);


			// Add character
			var characterModel = _scene.Root.QueryFirstByType<NursiaModelNode>();
			var swordScene = assetManager.LoadStoredScene("Scenes/Sword.scene");
			var swordModel = swordScene.Root.QueryFirstByType<NursiaModelNode>();
			_controllerService = new Character(characterModel, swordModel);

			_cameraMount = _scene.Root.QueryFirstById("_cameraMount");
			_mainCamera = _scene.Root.QueryFirstByType<Camera>();

			// Input system with locked mouse for FPS control
			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;
			_inputService.KeyDown += _inputService_KeyDown;
			_inputService.MouseLocked = true;

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// Nrs.DebugSettings.VisualizeBuffer = DebugVisualizeBuffer.ReflectionShadowMap;
			Nrs.GraphicsSettings.ShadowType = ShadowType.PCF;
			Nrs.GraphicsSettings.MaxShadowDistance = 100.0f;
			// Nrs.DebugSettings.DrawCamerasFrustums = true;

			// UI panel
			MyraEnvironment.Game = this;
			_desktop = new Desktop();
			_mainPanel = new MainPanel();
			_desktop.Root = _mainPanel;
		}

		/// <summary>Handles keyboard events (Escape toggles mouse lock).</summary>
		private void _inputService_KeyDown(object sender, KeyEventsArgs e)
		{
			// Escape toggles between locked mouse (FPS camera control) and free mouse (UI interaction)
			if (e.Key == Keys.Escape)
			{
				_inputService.MouseLocked = !_inputService.MouseLocked;
			}
		}

		/// <summary>Rotates character/camera based on mouse movement (when locked).</summary>
		private void _inputService_MouseMoved(object sender, InputEventArgs<Point> e)
		{
			if (!_inputService.MouseLocked)
				return;

			var playerRotation = ModelNode.Rotation;
			playerRotation.Y += -(int)((e.NewValue.X - e.OldValue.X) * MouseSensitivity);
			ModelNode.Rotation = playerRotation;

			var cameraRotation = _cameraMount.Rotation;
			cameraRotation.X += (int)((e.NewValue.Y - e.OldValue.Y) * MouseSensitivity);
			_cameraMount.Rotation = cameraRotation;
		}

		/// <summary>Updates game logic: input, animations, FPS counter.</summary>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_inputService.Update();

			// Process WASD movement
			var isRunning = false;
			var velocity = Vector3.Zero;

			if (_inputService.IsKeyDown(Keys.W))
			{
				velocity = ModelNode.GlobalTransform.Forward * -MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.S))
			{
				velocity = ModelNode.GlobalTransform.Forward * MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.A))
			{
				velocity = ModelNode.GlobalTransform.Right * MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.D))
			{
				velocity = ModelNode.GlobalTransform.Right * -MovementSpeed;
				isRunning = true;
			}

			if (_inputService.IsKeyDown(Keys.Space))
				_controllerService.Jump(velocity);

			if (_inputService.IsKeyDown(Keys.LeftShift))
				_controllerService.Slash();

			if (_inputService.IsKeyDown(Keys.R))
			{
				if (_controllerService.WeaponDrawn)
					_controllerService.SheathWeapon();
				else
					_controllerService.DrawWeapon();
			}

			if (isRunning)
				_controllerService.Run(velocity);
			else
				_controllerService.Idle();

			_controllerService.Update(gameTime.ElapsedGameTime);
			_scene.Update(gameTime);
		}

		/// <summary>Renders 3D scene and UI overlay.</summary>
		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			// Render the scene
			_scene.Render(_renderer, _mainCamera);

			// Update UI statistics
			_mainPanel._labelFPS.Text = $"FPS: {_fpsCounter.FramesPerSecond}";
			var stats = _renderer.Statistics;
			_mainPanel._labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
			_mainPanel._labelDrawCalls.Text = stats.DrawCalls.ToString();
			_mainPanel._labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();
			_mainPanel._labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
			_mainPanel._labelPassesDrawn.Text = stats.Passes.ToString();

			_desktop.Render();

			_fpsCounter.OnFrameDrawn();
		}
	}
}
