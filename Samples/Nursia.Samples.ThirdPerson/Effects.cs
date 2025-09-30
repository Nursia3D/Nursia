using Nursia.Utilities;
using System.IO;
using System.Text;

namespace Nursia.Samples.ThirdPerson
{
	internal static class Effects
	{
#if FNA
		private const string EffectsResourcePath = "Effects.FNA.bin";
#else
		private const string EffectsResourcePath = "Effects.MonoGameOGL.bin";
#endif

		public static byte[] GetEffectSource(string name, string[] defines = null)
		{
			var sb = new StringBuilder();

			sb.Append($"Nursia.Samples.ThirdPerson.{EffectsResourcePath}."); // Prefix
			sb.Append(name);

			if (defines != null)
			{
				// Apply defines to the effect name
				foreach (var def in defines)
				{
					sb.Append("_");
					sb.Append(def);
				}
			}

			sb.Append(".efb");

			// Read
			var assembly = typeof(Effects).Assembly;

			var r = assembly.GetManifestResourceNames();
			var ms = new MemoryStream();
			using (var input = assembly.GetManifestResourceStream(sb.ToString()))
			{
				input.CopyTo(ms);

				return ms.ToArray();
			}
		}
	}
}
