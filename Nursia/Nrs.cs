using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using Newtonsoft.Json.Linq;
using Nursia.Rendering;
using System;
using System.Reflection;

namespace Nursia
{
	public static partial class Nrs
	{
		private static Game _game;

		public static IEffectsSource EffectsSource = new StaticEffectsSource();

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

		public static GraphicsDevice GraphicsDevice
		{
			get
			{
				return Game.GraphicsDevice;
			}
		}

		public static SpriteFont DebugFont => Resources.DebugFont;

		public static Action<string> LogInfo = Console.WriteLine;
		public static Action<string> LogWarning = Console.WriteLine;
		public static Action<string> LogError = Console.WriteLine;

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