#region File Description
//-----------------------------------------------------------------------------
// FuelCellGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Nursia;
using Nursia.Rendering;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using Nursia.Shadows;
using System;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using MediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;

namespace FuelCell
{
	/// <summary>
	/// The available states of the game
	/// </summary>
	public enum GameState { Loading, Running, Won, Lost }

	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class FuelCellGame : Game
	{
		// Resources for drawing.
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private SpriteFont statsFont;
		private GameObject ground;
		private Random random;
		private GameState currentGameState = GameState.Loading;

		// Game objects
		private FuelCarrier fuelCarrier;
		private FuelCell[] fuelCells;
		private Barrier[] barriers;

		private GameObject boundingSphere;

		private int retrievedFuelCells = 0;
		private TimeSpan startTime, roundTimer, roundTime;
		private float aspectRatio;
		private IInputState inputState;
		private SceneRenderer _renderer;
		private Scene _scene;
		private DirectLight _light;


		public FuelCellGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			random = new Random();
			roundTime = GameConstants.RoundTime;
			graphics.PreferredBackBufferWidth = 1200;
			graphics.PreferredBackBufferHeight = 800;
			inputState = new InputState(this);
		}

		protected override void Initialize()
		{
			// Initialize the Game objects
			ground = new GameObject();
			boundingSphere = new GameObject();
			aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

			_scene = new Scene
			{
				Root = new SceneNode(),
				Camera = new Camera
				{
					ViewAngle = GameConstants.ViewAngle,
					NearPlane = GameConstants.NearClip,
					FarPlane = GameConstants.FarClip
				}
			};

			_renderer = new SceneRenderer(_scene);


			_light = new DirectLight
			{
				Rotation = new Vector3(320, 32, 0)
			};

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Initialize Nursia
			Nrs.Game = this;
			Nrs.GraphicsSettings.ShadowType = ShadowType.Simple;
			Nrs.GraphicsSettings.ShadowMapSize = ShadowMapSize.Size2048;
			// DebugSettings.DrawBoundingBoxes = true;

			// Create asset manager
			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(Utility.ExecutingAssemblyDirectory, "Assets"));

			// Load font
			statsFont = Nrs.DebugFont;

			// Load models
			ground.Model = (NursiaModelNode)assetManager.LoadSceneNode("Models/ground.scene");
			boundingSphere.Model = (NursiaModelNode)assetManager.LoadSceneNode("Models/sphere1uR.scene");

			// Initialize fuel cells
			fuelCells = new FuelCell[GameConstants.NumFuelCells];
			for (int index = 0; index < fuelCells.Length; index++)
			{
				fuelCells[index] = new FuelCell();
				fuelCells[index].LoadContent(assetManager, "Models/fuelcellmodel.scene");
			}

			// Initialize barriers
			barriers = new Barrier[GameConstants.NumBarriers];
			int randomBarrier = random.Next(3);
			string barrierName = null;

			for (int index = 0; index < barriers.Length; index++)
			{
				switch (randomBarrier)
				{
					case 0:
						barrierName = "Models/cube10uR";
						break;
					case 1:
						barrierName = "Models/cylinder10uR";
						break;
					case 2:
						barrierName = "Models/pyramid10uR";
						break;
				}

				barrierName += ".scene";
				barriers[index] = new Barrier();
				barriers[index].LoadContent(assetManager, barrierName);
				randomBarrier = random.Next(3);
			}

			PlaceFuelCellsAndBarriers();

			// Initialize fuel carrier
			fuelCarrier = new FuelCarrier();
			fuelCarrier.LoadContent(assetManager, "Models/fuelcarrier.scene");
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Update the InputState class.
			inputState.Update();

			// Allows the game to exit
			if (inputState.PlayerExit(PlayerIndex.One))
			{
				this.Exit();
			}

			// If the player has only just pressed the Enter key or has pressed the Start button
			if (currentGameState == GameState.Loading)
			{
				if (inputState.StartGame(PlayerIndex.One))
				{
					ResetGame(gameTime, aspectRatio);
				}
			}

			// Main gameplay running screen
			if ((currentGameState == GameState.Running))
			{
				fuelCarrier.Update(inputState, barriers);

				UpdateCamera();
				retrievedFuelCells = 0;
				foreach (FuelCell fuelCell in fuelCells)
				{
					fuelCell.Update(fuelCarrier.BoundingSphere);
					if (fuelCell.Retrieved)
					{
						retrievedFuelCells++;
					}
				}
				if (retrievedFuelCells == GameConstants.NumFuelCells)
				{
					currentGameState = GameState.Won;
				}
				roundTimer -= gameTime.ElapsedGameTime;
				if ((roundTimer < TimeSpan.Zero) && (retrievedFuelCells != GameConstants.NumFuelCells))
				{
					currentGameState = GameState.Lost;
				}
			}

			if ((currentGameState == GameState.Won) || (currentGameState == GameState.Lost))
			{
				// Gameplay has stopped and if audio is still playing, stop it.
				if (MediaPlayer.State == MediaState.Playing)
				{
					MediaPlayer.Stop();
				}

				// Reset the world for a new game
				if (inputState.StartGame(PlayerIndex.One))
				{
					ResetGame(gameTime, aspectRatio);
				}
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// Draws the game from background to foreground.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			switch (currentGameState)
			{
				case GameState.Loading:
					DrawSplashScreen();
					break;
				case GameState.Running:
					DrawGameplayScreen(gameTime);
					break;
				case GameState.Won:
					DrawWinOrLossScreen(GameConstants.StrGameWon);
					break;
				case GameState.Lost:
					DrawWinOrLossScreen(GameConstants.StrGameLost);
					break;
			}

			base.Draw(gameTime);
		}

		private void DrawGameplayScreen(GameTime gameTime)
		{
			var root = _scene.Root;
			root.Children.Clear();

			root.Children.Add(_light);

			// Draw the ground terrain model
			root.Children.Add(ground.Model);

			// Draw the fuel cells on the map
			foreach (FuelCell fuelCell in fuelCells)
			{
				if (!fuelCell.Retrieved)
				{
					root.Children.Add(fuelCell.Model);
				}
			}

			// Draw the barriers on the map
			foreach (Barrier barrier in barriers)
			{
				root.Children.Add(barrier.Model);
			}

			// Draw the player fuelcarrier on the map
			root.Children.Add(fuelCarrier.Model);

			_renderer.Render();

			DrawStats();
		}

		private void DrawSplashScreen()
		{
			float xOffsetText, yOffsetText;
			Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			Vector2 strCenter;

			graphics.GraphicsDevice.Clear(Color.SteelBlue);

			xOffsetText = yOffsetText = 0;
			Vector2 strInstructionsSize = statsFont.MeasureString(GameConstants.StrInstructions1);
			Vector2 strPosition;
			strCenter = new Vector2(strInstructionsSize.X / 2, strInstructionsSize.Y / 2);
			
			yOffsetText = (viewportSize.Y / 2 - strCenter.Y);
			xOffsetText = (viewportSize.X / 2 - strCenter.X);
			strPosition = new Vector2(xOffsetText, yOffsetText);

			spriteBatch.Begin();
			spriteBatch.DrawString(statsFont, GameConstants.StrInstructions1, strPosition, Color.White);

			strInstructionsSize = statsFont.MeasureString(GameConstants.StrInstructions2);
			strCenter = new Vector2(strInstructionsSize.X / 2, strInstructionsSize.Y / 2);
			yOffsetText = (viewportSize.Y / 2 - strCenter.Y) + statsFont.LineSpacing;
			xOffsetText = (viewportSize.X / 2 - strCenter.X);
			strPosition = new Vector2(xOffsetText, yOffsetText);

			spriteBatch.DrawString(statsFont, GameConstants.StrInstructions2, strPosition, Color.LightGray);
			spriteBatch.End();

			ResetRenderStates();
		}

		private void DrawWinOrLossScreen(string gameResult)
		{
			float xOffsetText, yOffsetText;
			Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			Vector2 strCenter;

			xOffsetText = yOffsetText = 0;
			Vector2 strResult = statsFont.MeasureString(gameResult);
			Vector2 strPlayAgainSize = statsFont.MeasureString(GameConstants.StrPlayAgain);
			Vector2 strPosition;
			strCenter = new Vector2(strResult.X / 2, strResult.Y / 2);

			yOffsetText = (viewportSize.Y / 2 - strCenter.Y);
			xOffsetText = (viewportSize.X / 2 - strCenter.X);
			strPosition = new Vector2(xOffsetText, yOffsetText);

			spriteBatch.Begin();
			spriteBatch.DrawString(statsFont, gameResult, strPosition, Color.Red);

			strCenter = new Vector2(strPlayAgainSize.X / 2, strPlayAgainSize.Y / 2);
			yOffsetText = (viewportSize.Y / 2 - strCenter.Y) + (float)statsFont.LineSpacing;
			xOffsetText = (viewportSize.X / 2 - strCenter.X);
			strPosition = new Vector2(xOffsetText, yOffsetText);
			spriteBatch.DrawString(statsFont, GameConstants.StrPlayAgain, strPosition, Color.AntiqueWhite);

			spriteBatch.End();

			ResetRenderStates();
		}

		private void DrawStats()
		{
			float xOffsetText, yOffsetText;
			string str1 = GameConstants.StrTimeRemaining;
			string str2 = GameConstants.StrCellsFound + retrievedFuelCells.ToString() + " of " + GameConstants.NumFuelCells.ToString();
			Rectangle rectSafeArea;

			str1 += (roundTimer.Seconds).ToString();

			//Calculate str1 position
			rectSafeArea = GraphicsDevice.Viewport.TitleSafeArea;

			xOffsetText = rectSafeArea.X;
			yOffsetText = rectSafeArea.Y;

			Vector2 strSize = statsFont.MeasureString(str1);
			Vector2 strPosition = new Vector2(xOffsetText + 10, yOffsetText);

			spriteBatch.Begin();
			spriteBatch.DrawString(statsFont, str1, strPosition, Color.White);
			strPosition.Y += strSize.Y;
			spriteBatch.DrawString(statsFont, str2, strPosition, Color.White);
			strPosition.Y += strSize.Y;
			spriteBatch.DrawString(statsFont, $"Effect Switches: {_renderer.Statistics.EffectsSwitches}", strPosition, Color.White);
			strPosition.Y += strSize.Y;
			spriteBatch.DrawString(statsFont, $"Draw Calls: {_renderer.Statistics.DrawCalls}", strPosition, Color.White);
			strPosition.Y += strSize.Y;
			spriteBatch.DrawString(statsFont, $"Vertices Drawn: {_renderer.Statistics.VerticesDrawn}", strPosition, Color.White);
			strPosition.Y += strSize.Y;
			spriteBatch.DrawString(statsFont, $"Primitives Drawn: {_renderer.Statistics.PrimitivesDrawn}", strPosition, Color.White);
			strPosition.Y += strSize.Y;
			spriteBatch.DrawString(statsFont, $"Passes: {_renderer.Statistics.Passes}", strPosition, Color.White);

			spriteBatch.End();

			ResetRenderStates();
		}

		private void ResetRenderStates()
		{
			//re-enable depth buffer after sprite batch disablement
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		}

		private void PlaceFuelCellsAndBarriers()
		{
			int min = GameConstants.MinDistance;
			int max = GameConstants.MaxDistance;
			Vector3 tempCenter;

			//place fuel cells
			foreach (FuelCell cell in fuelCells)
			{
				cell.Position = GenerateRandomPosition(min, max);
				tempCenter = cell.BoundingSphere.Center;
				tempCenter.X = cell.Position.X;
				tempCenter.Z = cell.Position.Z;
				cell.BoundingSphere = new BoundingSphere(tempCenter, cell.BoundingSphere.Radius);
				cell.Retrieved = false;
			}

			//place barriers
			foreach (Barrier barrier in barriers)
			{
				barrier.Position = GenerateRandomPosition(min, max);
				tempCenter = barrier.BoundingSphere.Center;
				tempCenter.X = barrier.Position.X;
				tempCenter.Z = barrier.Position.Z;
				barrier.BoundingSphere = new BoundingSphere(tempCenter, barrier.BoundingSphere.Radius);
			}
		}

		private Vector3 GenerateRandomPosition(int min, int max)
		{
			int xValue, zValue;
			do
			{
				xValue = random.Next(min, max);
				zValue = random.Next(min, max);
				if (random.Next(100) % 2 == 0)
					xValue *= -1;
				if (random.Next(100) % 2 == 0)
					zValue *= -1;

			} while (IsOccupied(xValue, zValue));

			return new Vector3(xValue, 0, zValue);
		}

		private bool IsOccupied(int xValue, int zValue)
		{
			foreach (GameObject currentObj in fuelCells)
			{
				if (((int)(MathHelper.Distance(xValue, currentObj.Position.X)) < 15) &&
					((int)(MathHelper.Distance(zValue, currentObj.Position.Z)) < 15))
				{
					return true;
				}
			}

			foreach (GameObject currentObj in barriers)
			{
				if (((int)(MathHelper.Distance(xValue, currentObj.Position.X)) < 15) &&
					((int)(MathHelper.Distance(zValue, currentObj.Position.Z)) < 15))
				{
					return true;
				}
			}
			return false;
		}

		private void ResetGame(GameTime gameTime, float aspectRatio)
		{
			fuelCarrier.Reset();
			UpdateCamera();
			InitializeGameField();

			retrievedFuelCells = 0;
			startTime = gameTime.TotalGameTime;
			roundTimer = roundTime;
			currentGameState = GameState.Running;
		}

		private void InitializeGameField()
		{
			// Initialize barriers

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(Utility.ExecutingAssemblyDirectory, "Assets"));

			barriers = new Barrier[GameConstants.NumBarriers];
			int randomBarrier = random.Next(3);
			string barrierName = null;

			for (int index = 0; index < GameConstants.NumBarriers; index++)
			{
				switch (randomBarrier)
				{
					case 0:
						barrierName = "Models/cube10uR";
						break;
					case 1:
						barrierName = "Models/cylinder10uR";
						break;
					case 2:
						barrierName = "Models/pyramid10uR";
						break;
				}

				barrierName += ".scene";

				barriers[index] = new Barrier();
				barriers[index].LoadContent(assetManager, barrierName);
				randomBarrier = random.Next(3);
			}
			PlaceFuelCellsAndBarriers();
		}

		private void UpdateCamera()
		{
			var avatarYaw = fuelCarrier.ForwardDirection;
			Matrix rotationMatrix = Matrix.CreateRotationY(avatarYaw);

			Vector3 transformedheadOffset = Vector3.Transform(GameConstants.AvatarHeadOffset, rotationMatrix);
			Vector3 transformedReference = Vector3.Transform(GameConstants.TargetOffset, rotationMatrix);

			var position = fuelCarrier.Position;
			Vector3 cameraPosition = position + transformedheadOffset;
			Vector3 cameraTarget = position + transformedReference;

			_scene.Camera.View = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
		}
	}
}
