using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.SceneGraph.Lights;
using System.Reflection;

namespace Nursia
{

	internal static class Resources
	{
		private static readonly AssetManager _assetManager = AssetManager.CreateResourceAssetManager(Assembly, "Resources");

		private static Texture2D _white, _waterNormals;
		private static Texture2D[] _ramps = new Texture2D[3];
		private static Texture2D[] _spots = new Texture2D[2];
		private static TextureCube _whiteCube;
		private static SpriteBatch _spriteBatch;
		private static SpriteFont _debugFont;

		private static Assembly Assembly
		{
			get
			{
				return typeof(Resources).Assembly;
			}
		}

		public static Texture2D White
		{
			get
			{
				if (_white == null)
				{
					_white = new Texture2D(Nrs.GraphicsDevice, 1, 1);
					_white.SetData(new[] { Color.White });
				}

				return _white;
			}
		}

		public static TextureCube WhiteCube
		{
			get
			{
				if (_whiteCube == null)
				{
					_whiteCube = new TextureCube(Nrs.GraphicsDevice, 1, false, SurfaceFormat.Color);
					_whiteCube.SetData(CubeMapFace.NegativeX, new[] { Color.White });
					_whiteCube.SetData(CubeMapFace.PositiveX, new[] { Color.White });
					_whiteCube.SetData(CubeMapFace.NegativeY, new[] { Color.White });
					_whiteCube.SetData(CubeMapFace.PositiveY, new[] { Color.White });
					_whiteCube.SetData(CubeMapFace.NegativeZ, new[] { Color.White });
					_whiteCube.SetData(CubeMapFace.PositiveZ, new[] { Color.White });
				}

				return _whiteCube;
			}
		}

		public static Texture2D WaterNormals
		{
			get
			{
				if (_waterNormals == null)
				{
					_waterNormals = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Images.WaterNoise.dds");
				}

				return _waterNormals;
			}
		}

		public static SpriteBatch SpriteBatch
		{
			get
			{
				if (_spriteBatch == null)
				{
					_spriteBatch = new SpriteBatch(Nrs.GraphicsDevice);
				}

				return _spriteBatch;
			}
		}

		public static SpriteFont DebugFont
		{
			get
			{
				if (_debugFont == null)
				{
					_debugFont = _assetManager.LoadSpriteFont(Nrs.GraphicsDevice, "Fonts/debugFont.fnt");
				}

				return _debugFont;
			}
		}

		public static Texture2D GetPointLightRamp(PointLightRamp ramp)
		{
			var idx = (int)ramp;
			if (_ramps[idx] != null)
			{
				return _ramps[idx];
			}

			Texture2D result;
			switch (ramp)
			{
				case PointLightRamp.Wide:
					result = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Images.RampWide.png");
					break;
				case PointLightRamp.Extreme:
					result = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Images.RampExtreme.png");
					break;
				default:
					result = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Images.Ramp.png");
					break;
			}

			_ramps[idx] = result;

			return result;
		}

		public static Texture2D GetSpotLightRamp(SpotLightRamp ramp)
		{
			var idx = (int)ramp;
			if (_spots[idx] != null)
			{
				return _spots[idx];
			}

			Texture2D result;
			switch (ramp)
			{
				case SpotLightRamp.Wide:
					result = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Images.SpotWide.png");
					break;
				default:
					result = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Images.Spot.png");
					break;
			}

			_spots[idx] = result;

			return result;
		}
	}
}