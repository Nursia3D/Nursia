using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Nursia.Editor.UI;
using Nursia.SceneGraph;
using System.Linq;

namespace Nursia.Editor
{
	public class StudioGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Desktop _desktop = null;
		private MainForm _mainForm;
		private readonly State _state;
		private GameTime _gameTime;

		public static StudioGame Instance { get; private set; }
		public static MainForm MainForm => Instance._mainForm;
		public static GameTime GameTime => Instance._gameTime;


		public StudioGame(string path)
		{
			Instance = this;

			Configuration.ProjectFolder = path;
			Configuration.LastAssetsFolder = path;

			// Restore state
			_state = State.Load();
			if (_state != null)
			{
				NursiaEditorOptions.ShowGrid = _state.ShowGrid;
				NursiaEditorOptions.DrawShadowMap = _state.DrawShadowMap;

				if (_state.GraphicsSettings != null)
				{
					Nrs.GraphicsSettings = _state.GraphicsSettings;
				}

				if (_state.DebugSettings != null)
				{
					Nrs.DebugSettings = _state.DebugSettings;
				}

				if (_state.ReferencedAssemblies != null && _state.ReferencedAssemblies.Length > 0)
				{
					AssemblyReferenceManager.LoadAssemblies(_state.ReferencedAssemblies);
				}
			}

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800,
				GraphicsProfile = GraphicsProfile.HiDef,
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}

			if (_state != null)
			{
				_graphics.PreferredBackBufferWidth = _state.Size.X;
				_graphics.PreferredBackBufferHeight = _state.Size.Y;
			}
			else
			{
				_graphics.PreferredBackBufferWidth = 1280;
				_graphics.PreferredBackBufferHeight = 800;
			}

			// DebugSettings.DrawCamerasFrustums = true;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// UI
			MyraEnvironment.Game = this;
			Nrs.Game = this;

			/*			Nrs.ExternalEffects = new FolderWatcher(@"D:\Projects\Nursia\Nursia\Effects")
						{
							BinaryFolder = @"D:\Projects\Nursia\Nursia\Effects\FNA\bin"
						};*/

			_mainForm = new MainForm();

			if (_state != null)
			{
				_mainForm.ShowCameraPosition = _state.ShowCameraPosition;
			}

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainForm);

			if (_state != null)
			{
				_mainForm._topSplitPane.SetSplitterPosition(0, _state != null ? _state.TopSplitterPosition : 0.75f);
				_mainForm._leftSplitPane.SetSplitterPosition(0, _state != null ? _state.LeftSplitterPosition : 0.5f);
			}

			_mainForm.OpenFolder(Configuration.ProjectFolder);

			if (_state != null && _state.OpenFiles != null)
			{
				foreach (var file in _state.OpenFiles)
				{
					_mainForm.OpenFile(file);
				}
			}

			NodesRegistry.AddAssembly(typeof(SceneNode).Assembly);

			GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			_gameTime = gameTime;

			GraphicsDevice.Clear(Color.Black);

			_desktop.Render();
		}

		public void SaveProject()
		{
			var state = new State
			{
				Size = new Point(GraphicsDevice.PresentationParameters.BackBufferWidth,
					GraphicsDevice.PresentationParameters.BackBufferHeight),
				TopSplitterPosition = _mainForm._topSplitPane.GetSplitterPosition(0),
				LeftSplitterPosition = _mainForm._leftSplitPane.GetSplitterPosition(0),
				OpenFiles = _mainForm.OpenFiles.ToArray(),
				ShowGrid = NursiaEditorOptions.ShowGrid,
				DrawShadowMap = NursiaEditorOptions.DrawShadowMap,
				ShowCameraPosition = _mainForm.ShowCameraPosition,
				GraphicsSettings = Nrs.GraphicsSettings,
				DebugSettings = Nrs.DebugSettings,
				ReferencedAssemblies = (from r in AssemblyReferenceManager.References select r.SourcePath).ToArray()
			};

			state.Save();
		}

		protected override void EndRun()
		{
			base.EndRun();

			SaveProject();
		}
	}
}