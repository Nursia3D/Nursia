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

		public virtual EffectBinding GetShadowTechnique(DrMeshPart mesh, bool instancing)
		{
			return UtilityEffects.GetShadowEffect(mesh != null && mesh.Skin != null, instancing);
		}

		public abstract EffectBinding GetColorTechnique(DrMeshPart mesh, LightTechnique technique, bool shadow, bool translucent, bool clipPlane, bool instancing);
		public abstract IMaterial Clone();
	}
}
