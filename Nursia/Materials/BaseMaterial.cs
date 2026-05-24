using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Materials
{
	/// <summary>
	/// Base class for all materials in the Nursia engine.
	/// </summary>
	public abstract class BaseMaterial : IMaterial
	{
		/// <summary>
		/// Gets the flags that describe this material's properties and capabilities.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public abstract MaterialFlags Flags { get; }

		/// <summary>
		/// Gets or sets the blend state for this material.
		/// </summary>
		[Category("States")]
		public BlendState BlendState { get; set; }

		/// <summary>
		/// Gets or sets the depth stencil state for this material.
		/// </summary>
		[Category("States")]
		public DepthStencilState DepthStencilState { get; set; }

		/// <summary>
		/// Gets or sets the rasterizer state for this material.
		/// </summary>
		[Category("States")]
		public RasterizerState RasterizerState { get; set; }

		/// <summary>
		/// Gets the effect binding for rendering shadows using this material.
		/// </summary>
		/// <param name="materialTechnique">The material technique to use for shadow rendering.</param>
		/// <returns>The effect binding for shadow rendering.</returns>
		public virtual EffectBinding GetShadowTechnique(MaterialTechnique materialTechnique)
		{
			return UtilityEffects.GetShadowEffect(materialTechnique);
		}

		/// <summary>
		/// Gets the effect binding for rendering color/final output with this material.
		/// </summary>
		/// <param name="materialTechnique">The material technique to use.</param>
		/// <param name="lightTechnique">The type of lighting to apply.</param>
		/// <param name="shadow">Whether shadows should be rendered.</param>
		/// <param name="translucent">Whether the material is rendered as translucent.</param>
		/// <param name="normalMapping">Whether normal mapping should be applied.</param>
		/// <param name="clipPlane">Whether clip plane is in use.</param>
		/// <returns>The effect binding for color rendering.</returns>
		public abstract EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane);

		/// <summary>
		/// Creates a copy of this material instance.
		/// </summary>
		/// <returns>A cloned instance of this material.</returns>
		public abstract IMaterial Clone();
	}
}
