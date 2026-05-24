using AssetManagementBase;
using DigitalRiseModel;
using Nursia.SceneGraph;

namespace Nursia
{
	/// <summary>
	/// Extension methods for loading Nursia-specific assets through the AssetManager.
	/// </summary>
	public static class NursiaAssetsExt
	{
		private readonly static AssetLoader<Scene> _sceneLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);
			return Scene.ReadFromString(data, manager);
		};

		/// <summary>
		/// Loads a stored scene from the asset manager and returns a clone.
		/// </summary>
		/// <param name="assetManager">The asset manager to load from.</param>
		/// <param name="path">The path to the scene asset.</param>
		/// <returns>A cloned scene instance.</returns>
		public static Scene LoadStoredScene(this AssetManager assetManager, string path) =>
			assetManager.UseLoader(_sceneLoader, path).Clone();

		/// <summary>
		/// Loads a scene node from the asset manager.
		/// </summary>
		/// <param name="assetManager">The asset manager to load from.</param>
		/// <param name="path">The path to the scene asset.</param>
		/// <returns>The root node of the loaded scene.</returns>
		public static SceneNode LoadSceneNode(this AssetManager assetManager, string path) =>
			assetManager.LoadStoredScene(path).Root;

		/// <summary>
		/// Loads a model and creates a NursiaModelNode containing it.
		/// </summary>
		/// <param name="assetManager">The asset manager to load from.</param>
		/// <param name="modelPath">The path to the model asset.</param>
		/// <param name="flags">The flags specifying how to load the model.</param>
		/// <returns>A new NursiaModelNode containing the loaded model.</returns>
		public static NursiaModelNode LoadModelNode(this AssetManager assetManager, string modelPath, ModelLoadFlags flags = ModelLoadFlags.EnsureUVs)
		{
			var model = assetManager.LoadModel(Nrs.GraphicsDevice, modelPath, flags);

			return new NursiaModelNode
			{
				Model = model
			};
		}
	}
}
