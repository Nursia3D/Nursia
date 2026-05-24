using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Nursia
{
	/// <summary>
	/// Default effects registry that simply loads precompiled effects from assembly resources
	/// </summary>
	public class StaticEffectsSource : IEffectsSource
	{
#if FNA
		private const string EffectsResourcePath = "Effects.FNA.bin";
#else
		private const string EffectsResourcePath = "Effects.MonoGameOGL.bin";
#endif

		private Dictionary<string, AssetManager> _assetsManagers = new Dictionary<string, AssetManager>();

		/// <summary>
		/// Gets an effect from the specified assembly with the given name.
		/// </summary>
		/// <param name="assembly">The assembly to load the effect from.</param>
		/// <param name="name">The name of the effect.</param>
		/// <param name="defines">A dictionary of preprocessor defines to apply.</param>
		/// <returns>The loaded effect.</returns>
		public Effect GetEffect(Assembly assembly, string name, Dictionary<string, string> defines)
		{
			AssetManager assetManager;

			var key = assembly.GetName().Name;
			if (!_assetsManagers.TryGetValue(key, out assetManager))
			{
				assetManager = AssetManager.CreateResourceAssetManager(assembly, EffectsResourcePath);
				_assetsManagers[key] = assetManager;
			}

			name = Path.ChangeExtension(name, "efb");
			return assetManager.LoadEffect(Nrs.GraphicsDevice, name, defines);
		}

		/// <summary>
		/// Determines whether the specified effect is valid.
		/// </summary>
		/// <remarks>
		/// Static effects source always returns true.
		/// </remarks>
		/// <param name="effect">The effect to validate.</param>
		/// <returns>Always returns <c>true</c>.</returns>
		public bool IsEffectValid(Effect effect) => true;

		/// <summary>
		/// Updates the specified effect.
		/// </summary>
		/// <remarks>
		/// Static effects source does not support effect updates.
		/// </remarks>
		/// <param name="effect">The effect to update.</param>
		/// <returns>Not implemented.</returns>
		public Effect UpdateEffect(Effect effect)
		{
			throw new System.NotImplementedException();
		}
	}
}
