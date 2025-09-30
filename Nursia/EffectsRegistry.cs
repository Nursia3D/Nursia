using Nursia.Rendering;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Nursia
{
	internal static class EffectsRegistry
	{
		private class StoredEffect
		{
			public EffectBinding EffectBinding;
			public Assembly Assembly;
			public string Name;
			public Dictionary<string, string> Defines;
		}

		private static Dictionary<string, StoredEffect> _storedEffects = new Dictionary<string, StoredEffect>();

		private static string BuildEffectKey(Assembly assembly, string name, Dictionary<string, string> defines = null)
		{
			var sb = new StringBuilder();

			var assemblyName = assembly.GetName().Name;

			sb.Append(assemblyName);
			sb.Append("/");
			sb.Append(name);

			if (defines != null && defines.Count > 0)
			{
				var keys = (from def in defines.Keys orderby def select def).ToArray();
				for (var i = 0; i < keys.Length; ++i)
				{
					sb.Append("_");

					var k = keys[i];
					sb.Append(k);
					var value = defines[k];
					if (value != "1")
					{
						sb.Append("_");
						sb.Append(value);
					}
				}
			}

			return sb.ToString();
		}

		public static EffectBinding GetEffectBinding(Assembly assembly, string name, Dictionary<string, string> defines = null)
		{
			var key = BuildEffectKey(assembly, name, defines);

			StoredEffect result;
			if (_storedEffects.TryGetValue(key, out result))
			{
				return result.EffectBinding;
			}

			var effect = Nrs.EffectsSource.GetEffect(assembly, name, defines);
			var binding = new EffectBinding
			{
				Effect = effect
			};

			result = new StoredEffect
			{
				EffectBinding = binding,
				Assembly = assembly,
				Name = name,
				Defines = defines,
			};

			_storedEffects[key] = result;

			Nrs.LogInfo($"Effect '{key}' was assigned id {binding.BatchId}");

			return binding;
		}

		public static EffectBinding GetStockEffectBinding(string name, Dictionary<string, string> defines = null)
		{
			return GetEffectBinding(typeof(EffectsRegistry).Assembly, name, defines);
		}
	}
}
