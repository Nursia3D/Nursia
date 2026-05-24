using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;

namespace Nursia
{
	/// <summary>
	/// Interface for sources that provide and manage effects in the Nursia engine.
	/// </summary>
	public interface IEffectsSource
	{
		/// <summary>
		/// Determines whether the specified effect is valid for use in the engine.
		/// </summary>
		/// <param name="effect">The effect to validate.</param>
		/// <returns><c>true</c> if the effect is valid; otherwise, <c>false</c>.</returns>
		bool IsEffectValid(Effect effect);

		/// <summary>
		/// Updates the specified effect with current engine state.
		/// </summary>
		/// <param name="effect">The effect to update.</param>
		/// <returns>The updated effect.</returns>
		Effect UpdateEffect(Effect effect);

		/// <summary>
		/// Gets an effect from the specified assembly with the given name and defines.
		/// </summary>
		/// <param name="assembly">The assembly to load the effect from.</param>
		/// <param name="name">The name of the effect.</param>
		/// <param name="defines">A dictionary of preprocessor defines to apply.</param>
		/// <returns>The requested effect.</returns>
		Effect GetEffect(Assembly assembly, string name, Dictionary<string, string> defines);
	}
}
