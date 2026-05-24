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
	/// Represents a complete 3D scene in the Nursia engine.
	/// </summary>
	public partial class Scene : IHasExternalAssets
	{
		/// <summary>
		/// Gets or sets the camera for this scene.
		/// </summary>
		[Browsable(false)]
		public Camera Camera { get; set; }

		/// <summary>
		/// Gets or sets the game time for this scene.
		/// </summary>
		public GameTime GameTime { get; private set; }

		/// <summary>
		/// Gets or sets the render environment that defines lighting and atmosphere for this scene.
		/// </summary>
		public RenderEnvironment RenderEnvironment { get; set; } = RenderEnvironment.Default.Clone();

		/// <summary>
		/// Gets or sets the root node of the scene graph.
		/// </summary>
		public SceneNode Root { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Scene"/> class with a default camera.
		/// </summary>
		public Scene()
		{
			Camera = new Camera();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Scene"/> class with the specified root node.
		/// </summary>
		/// <param name="node">The root scene node.</param>
		public Scene(SceneNode node)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Scene"/> class with the specified root node and camera.
		/// </summary>
		/// <param name="node">The root scene node.</param>
		/// <param name="camera">The camera for the scene.</param>
		public Scene(SceneNode node, Camera camera)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
			Camera = camera ?? throw new ArgumentNullException(nameof(camera));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Scene"/> class with the specified root node, camera, and render environment.
		/// </summary>
		/// <param name="node">The root scene node.</param>
		/// <param name="camera">The camera for the scene.</param>
		/// <param name="env">The render environment for the scene.</param>
		public Scene(SceneNode node, Camera camera, RenderEnvironment env)
		{
			Root = node ?? throw new ArgumentNullException(nameof(node));
			Camera = camera ?? throw new ArgumentNullException(nameof(camera));
			RenderEnvironment = env ?? throw new ArgumentNullException(nameof(env));
		}

		/// <summary>
		/// Saves this scene to a file at the specified path.
		/// </summary>
		/// <param name="path">The file path to save to.</param>
		public void SaveToFile(string path)
		{
			JsonExtensions.SerializeToFile(path, this);
		}

		/// <summary>
		/// Creates a complete copy of this scene.
		/// </summary>
		/// <returns>A cloned scene with all nodes, camera, and environment duplicated.</returns>
		public Scene Clone()
		{
			return new Scene(Root.Clone(), (Camera)Camera.Clone(), RenderEnvironment.Clone());
		}

		/// <summary>
		/// Reads a scene from a JSON string and loads its external assets.
		/// </summary>
		/// <param name="data">The JSON string containing the scene data.</param>
		/// <param name="assetManager">The asset manager used to load external resources.</param>
		/// <returns>The deserialized and loaded scene.</returns>
		public static Scene ReadFromString(string data, AssetManager assetManager)
		{
			var result = JsonExtensions.DeserializeFromString<Scene>(data);

			result.Load(assetManager);

			return result;
		}

		/// <summary>
		/// Reads a scene from a file and loads its external assets.
		/// </summary>
		/// <param name="path">The file path to read the scene from.</param>
		/// <param name="assetManager">The asset manager used to load external resources.</param>
		/// <returns>The deserialized and loaded scene.</returns>
		public static Scene ReadFromFile(string path, AssetManager assetManager)
		{
			var data = File.ReadAllText(path);
			return ReadFromString(data, assetManager);
		}

		/// <summary>
		/// Loads all external assets referenced by this scene and its child nodes.
		/// </summary>
		/// <param name="assetManager">The asset manager to load resources from.</param>
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
