using AssetManagementBase;
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
	public class StoredScene: IHasExternalAssets
	{
		[Browsable(false)]
		public Camera Camera { get; set; }
		public RenderEnvironment RenderEnvironment { get; set; } = RenderEnvironment.Default.Clone();
		public SceneNode Root { get; set; }

		public StoredScene()
		{
			Camera = new Camera();
		}

		public StoredScene(SceneNode node)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
		}

		public StoredScene(SceneNode node, Camera camera)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
			Camera = camera ?? throw new ArgumentNullException(nameof(camera));
		}

		public StoredScene(SceneNode node, Camera camera, RenderEnvironment env)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
			Camera = camera ?? throw new ArgumentNullException(nameof(camera));
			RenderEnvironment = env ?? throw new ArgumentNullException(nameof(env));
		}

		public void SaveToFile(string path)
		{
			JsonExtensions.SerializeToFile(path, this);
		}

		public StoredScene Clone()
		{
			return new StoredScene(Root.Clone(), (Camera)Camera.Clone(), RenderEnvironment.Clone());
		}

		public static StoredScene ReadFromString(string data, AssetManager assetManager)
		{
			var result = JsonExtensions.DeserializeFromString<StoredScene>(data);

			result.Load(assetManager);

			return result;
		}

		public static StoredScene ReadFromFile(string path, AssetManager assetManager)
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
	}
}
