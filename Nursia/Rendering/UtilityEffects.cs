using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal static class UtilityEffects
	{
		private static readonly EffectBinding[] _shadowEffects = new EffectBinding[4];
		private static readonly EffectBinding[] _depthEffects = new EffectBinding[4];

		public static EffectBinding GetShadowEffect(bool skinning, bool instancing)
		{
			var key = 0;
			if (skinning)
			{
				key |= 1;
			}

			if (instancing)
			{
				key |= 2;
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

			if (instancing)
			{
				defines["INSTANCED"] = "1";
			}

			binding = EffectsRegistry.GetStockEffectBinding("Shadow", defines);
			_shadowEffects[key] = binding;

			return binding;
		}

		public static EffectBinding GetDepthEffect(bool skinning, bool instancing)
		{
			var key = 0;
			if (skinning)
			{
				key |= 1;
			}

			if (instancing)
			{
				key |= 2;
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

			if (instancing)
			{
				defines["INSTANCED"] = "1";
			}

			binding = EffectsRegistry.GetStockEffectBinding("Depth", defines);
			_depthEffects[key] = binding;

			return binding;
		}
	}
}
