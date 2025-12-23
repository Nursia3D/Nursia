using AssetManagementBase;
using Microsoft.Xna.Framework;
using Nursia.Env;
using Nursia.Serialization;
using Nursia.Utilities;
using System;
using System.ComponentModel;
using System.IO;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// Class for storing a SceneNode
	/// </summary>
	public partial class Scene : IHasExternalAssets
	{
		[Browsable(false)]
		public Camera Camera { get; set; }

		public GameTime GameTime { get; private set; }

		public RenderEnvironment RenderEnvironment { get; set; } = RenderEnvironment.Default.Clone();

		public SceneNode Root { get; set; }

		public Scene()
		{
			Camera = new Camera();
		}

		public Scene(SceneNode node)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
		}

		public Scene(SceneNode node, Camera camera)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
			Camera = camera ?? throw new ArgumentNullException(nameof(camera));
		}

		public Scene(SceneNode node, Camera camera, RenderEnvironment env)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
			Camera = camera ?? throw new ArgumentNullException(nameof(camera));
			RenderEnvironment = env ?? throw new ArgumentNullException(nameof(env));
		}

		public void SaveToFile(string path)
		{
			JsonExtensions.SerializeToFile(path, this);
		}

		public Scene Clone()
		{
			return new Scene(Root.Clone(), (Camera)Camera.Clone(), RenderEnvironment.Clone());
		}

		public static Scene ReadFromString(string data, AssetManager assetManager)
		{
			var result = JsonExtensions.DeserializeFromString<Scene>(data);

			result.Load(assetManager);

			return result;
		}

		public static Scene ReadFromFile(string path, AssetManager assetManager)
		{
			var data = File.ReadAllText(path);
			return ReadFromString(data, assetManager);
		}

		public void Load(AssetManager assetManager)
		{
			Root?.Load(assetManager);
			Camera?.Load(assetManager);
			RenderEnvironment?.Load(assetManager);
		}

		private static Action<SceneNode, GameTime> _updateHandler = new Action<SceneNode, GameTime>((n, t) => n.UpdateHandler?.Invoke(t));


		public void Update(GameTime gameTime)
		{
			Root.Traverse(_updateHandler, gameTime);
		}
	}
}
