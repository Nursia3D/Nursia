using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Materials
{
	public abstract class BaseMaterial : IMaterial
	{
		[Browsable(false)]
		[JsonIgnore]
		public abstract MaterialFlags Flags { get; }

		[Category("States")]
		public BlendState BlendState { get; set; }

		[Category("States")]
		public DepthStencilState DepthStencilState { get; set; }

		[Category("States")]
		public RasterizerState RasterizerState { get; set; }

		public virtual EffectBinding GetShadowTechnique(MaterialTechnique materialTechnique)
		{
			return UtilityEffects.GetShadowEffect(materialTechnique);
		}

		public abstract EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane);
		public abstract IMaterial Clone();
	}
}
