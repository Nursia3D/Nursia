using Nursia.SceneGraph.Lights;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal static class UtilityEffects
	{
		private static readonly EffectBinding[] _shadowEffects = new EffectBinding[2];
		private static readonly EffectBinding[] _depthEffects = new EffectBinding[2];

		public static EffectBinding GetShadowEffect(bool skinning)
		{
			var key = 0;
			if (skinning)
			{
				key |= 1;
			}

			var binding = _shadowEffects[key];
			if (binding != null)
			{
				return binding;
			}

			var defines = new Dictionary<string, string>();
			if (skinning)
			{
				defines["SKINNED"] = "1";
			}

			binding = EffectsRegistry.GetStockEffectBinding("Shadow", defines);
			_shadowEffects[key] = binding;

			return binding;
		}

		public static EffectBinding GetDepthEffect(bool skinning)
		{
			var key = 0;
			if (skinning)
			{
				key |= 1;
			}

			var binding = _depthEffects[key];
			if (binding != null)
			{
				return binding;
			}

			var defines = new Dictionary<string, string>();
			if (skinning)
			{
				defines["SKINNED"] = "1";
			}

			binding = EffectsRegistry.GetStockEffectBinding("Depth", defines);
			_depthEffects[key] = binding;

			return binding;
		}
	}
}
