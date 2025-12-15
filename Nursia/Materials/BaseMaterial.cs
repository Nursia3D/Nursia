using DigitalRiseModel;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Materials
{
	public abstract class BaseMaterial: IMaterial
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

		public virtual EffectBinding GetShadowTechnique(DrMeshPart mesh)
		{
			return UtilityEffects.GetShadowEffect(mesh != null && mesh.Skin != null);
		}

		public abstract EffectBinding GetColorTechnique(LightTechnique technique, bool shadow, bool translucent, DrMeshPart mesh, bool clipPlane);
		public abstract IMaterial Clone();
	}
}
