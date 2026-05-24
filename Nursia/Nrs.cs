using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Nursia.Rendering;
using System;
using System.Reflection;

#if MONOGAME

using MonoGame.Framework.Utilities;

#endif

namespace Nursia
{
	/// <summary>
	/// Main entry point and static configuration for the Nursia 3D engine.
	/// </summary>
	public static partial class Nrs
	{
		private static Game _game;

		/// <summary>
		/// Gets or sets the effects source used by the engine.
		/// </summary>
		public static IEffectsSource EffectsSource = new StaticEffectsSource();

		/// <summary>
		/// Gets or sets the MonoGame Game instance for the Nursia engine.
		/// Must be set before using any Nursia functionality.
		/// </summary>
		/// <remarks>
		/// Throws an exception if accessed before being set.
		/// </remarks>
		public static Game Game
		{
			get
			{
				if (_game == null)
				{
					throw new Exception("Nrs.Game is null. Please, set it to the Game instance before using Nursia.");
				}

				return _game;
			}

			set
			{
				SetGame(value, true);
			}
		}

		internal static void SetGame(Game game, bool checkPlatform)
		{
			if (_game == game)
			{
				return;
			}

#if MONOGAME
			if (checkPlatform)
			{
				if (PlatformInfo.GraphicsBackend != GraphicsBackend.OpenGL)
				{
					throw new NotSupportedException("Only OpenGL MonoGame backend is supported for now");
				}
			}
#endif

			if (_game != null)
			{
				_game.Disposed -= GameOnDisposed;
			}

			_game = game;
			DebugShapeRenderer.Initialize(GraphicsDevice);

			if (_game != null)
			{
				_game.Disposed += GameOnDisposed;
			}
		}

		/// <summary>
		/// Gets the graphics device from the current Game instance.
		/// </summary>
		public static GraphicsDevice GraphicsDevice
		{
			get
			{
				return Game.GraphicsDevice;
			}
		}

		/// <summary>
		/// Gets the debug font used for on-screen debugging text.
		/// </summary>
		public static SpriteFont DebugFont => Resources.DebugFont;

		/// <summary>
		/// Gets or sets the action used for info-level logging.
		/// </summary>
		public static Action<string> LogInfo = Console.WriteLine;

		/// <summary>
		/// Gets or sets the action used for warning-level logging.
		/// </summary>
		public static Action<string> LogWarning = Console.WriteLine;

		/// <summary>
		/// Gets or sets the action used for error-level logging.
		/// </summary>
		public static Action<string> LogError = Console.WriteLine;

		/// <summary>
		/// Gets the version number of the Nursia engine assembly.
		/// </summary>
		public static string Version
		{
			get
			{
				var assembly = typeof(Nrs).GetTypeInfo().Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

		private static void GameOnDisposed(object sender, EventArgs eventArgs)
		{
		}
	}
}