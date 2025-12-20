using AssetManagementBase;
using DigitalRiseModel;
using Nursia.SceneGraph;

namespace Nursia
{
	public static class NursiaAssetsExt
	{
		private readonly static AssetLoader<Scene> _sceneLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);
			return Scene.ReadFromString(data, manager);
		};

		public static Scene LoadStoredScene(this AssetManager assetManager, string path) =>
			assetManager.UseLoader(_sceneLoader, path).Clone();

		public static SceneNode LoadSceneNode(this AssetManager assetManager, string path) =>
			assetManager.LoadStoredScene(path).Root;

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
