using AssetManagementBase;
using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using Nursia.SceneGraph;

namespace Nursia.Editor
{
	internal static class Resources
	{
		private static readonly AssetManager _assetManager = AssetManager.CreateResourceAssetManager(typeof(Resources).Assembly, "Nursia.Editor.Assets", false);
		private static Texture2D _iconDirectionalLight, _iconCamera;
		private static SceneNode _modelAxises;
		private static SceneNode _directLightArrow;
		private static SceneNode _pointLightSphere;
		private static FontSystem _fontSystem;

		public static FontSystem DefaultFontSystem
		{
			get
			{
				if (_fontSystem == null)
				{
					_fontSystem = XNAssetsExtFontStashSharp.LoadFontSystem(_assetManager, "Fonts/Inter-Regular.ttf");
				}

				return _fontSystem;
			}
		}

		public static SpriteFontBase ErrorFont => DefaultFontSystem.GetFont(32);

		public static Texture2D IconDirectionalLight
		{
			get
			{
				if (_iconDirectionalLight == null)
				{
					_iconDirectionalLight = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Icons/DirectionalLight.png");
				}

				return _iconDirectionalLight;
			}
		}

		public static Texture2D IconCamera
		{
			get
			{
				if (_iconCamera == null)
				{
					_iconCamera = _assetManager.LoadTexture2D(Nrs.GraphicsDevice, "Icons/Camera.png");
				}

				return _iconCamera;
			}
		}

		public static SceneNode ModelAxises
		{
			get
			{
				if (_modelAxises == null)
				{
					_modelAxises = _assetManager.LoadSceneNode("Scenes/axises.scene");
				}

				return _modelAxises;
			}
		}

		public static SceneNode DirectLightArrow
		{
			get
			{
				if (_directLightArrow == null)
				{
					_directLightArrow = _assetManager.LoadSceneNode("Scenes/directLightArrow.scene");
				}

				return _directLightArrow;
			}
		}

		public static SceneNode PointLightSphere
		{
			get
			{
				if (_pointLightSphere == null)
				{
					_pointLightSphere = _assetManager.LoadSceneNode("Scenes/pointLightSphere.scene");
				}

				return _pointLightSphere;
			}
		}
	}
}
