using Nursia.Materials;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	internal static class UtilityEffects
	{
		private static readonly EffectBinding[] _shadowEffects = new EffectBinding[3];
		private static readonly EffectBinding[] _depthEffects = new EffectBinding[4];

		public static EffectBinding GetShadowEffect(MaterialTechnique materialTechnique)
		{
			var key = (int)materialTechnique;

			var binding = _shadowEffects[key];
			if (binding != null)
			{
				return binding;
			}

			var defines = new Dictionary<string, string>();
			switch (materialTechnique)
			{
				case MaterialTechnique.Skinned:
					defines["SKINNED"] = "1";
					break;
				case MaterialTechnique.Instanced:
					defines["INSTANCED"] = "1";
					break;
			}

			binding = EffectsRegistry.GetStockEffectBinding("Shadow", defines);
			_shadowEffects[key] = binding;

			return binding;
		}

		public static EffectBinding GetDepthEffect(MaterialTechnique materialTechnique)
		{
			var key = (int)materialTechnique;

			var binding = _shadowEffects[key];
			if (binding != null)
			{
				return binding;
			}

			var defines = new Dictionary<string, string>();
			switch (materialTechnique)
			{
				case MaterialTechnique.Skinned:
					defines["SKINNED"] = "1";
					break;
				case MaterialTechnique.Instanced:
					defines["INSTANCED"] = "1";
					break;
			}

			binding = EffectsRegistry.GetStockEffectBinding("Depth", defines);
			_depthEffects[key] = binding;

			return binding;
		}
	}
}
