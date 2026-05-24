using AssetManagementBase;

namespace Nursia.Serialization
{
	/// <summary>
	/// Interface for objects that have external assets that need to be loaded.
	/// </summary>
	public interface IHasExternalAssets
	{
		/// <summary>
		/// Loads all external assets referenced by this object.
		/// </summary>
		/// <param name="assetManager">The asset manager to load resources from.</param>
		void Load(AssetManager assetManager);
	}
}
