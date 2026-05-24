using DigitalRiseModel;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.SceneGraph.Lights;
using System;

namespace Nursia.Materials
{
	/// <summary>
	/// Specifies the light technique to apply during rendering.
	/// </summary>
	public enum LightTechnique
	{
		/// <summary>Directional light.</summary>
		Direct,

		/// <summary>Point light.</summary>
		Point,

		/// <summary>Spot light.</summary>
		Spot
	}

	/// <summary>
	/// Specifies the material technique for vertex processing.
	/// </summary>
	public enum MaterialTechnique
	{
		/// <summary>Standard rendering.</summary>
		Ordinary,

		/// <summary>Skinned mesh rendering.</summary>
		Skinned,

		/// <summary>Instanced rendering.</summary>
		Instanced
	}

	/// <summary>
	/// Flags that describe material properties and capabilities.
	/// </summary>
	[Flags]
	public enum MaterialFlags
	{
		/// <summary>No flags set.</summary>
		None = 0,

		/// <summary>Material accepts directional light.</summary>
		AcceptsDirectionalLight = 1 << 0,

		/// <summary>Material accepts point light.</summary>
		AcceptsPointLight = 1 << 1,

		/// <summary>Material accepts spot light.</summary>
		AcceptsSpotLight = 1 << 2,

		/// <summary>Material casts shadows.</summary>
		CastsShadows = 1 << 3,

		/// <summary>Material accepts/receives shadows.</summary>
		AcceptsShadows = 1 << 4,

		/// <summary>Material is transparent.</summary>
		IsTransparent = 1 << 5,

		/// <summary>Material requires a depth buffer.</summary>
		RequiresDepthBuffer = 1 << 6,

		/// <summary>Material requires a screen texture.</summary>
		RequiresScreenTexture = 1 << 7,

		/// <summary>Material accepts any type of light.</summary>
		AcceptsLight = AcceptsDirectionalLight | AcceptsPointLight | AcceptsSpotLight
	}

	/// <summary>
	/// Defines the interface for materials used in the Nursia rendering system.
	/// </summary>
	public interface IMaterial
	{
		/// <summary>
		/// Gets the flags that describe this material's properties and capabilities.
		/// </summary>
		MaterialFlags Flags { get; }

		/// <summary>
		/// Gets the blend state for rendering with this material.
		/// </summary>
		BlendState BlendState { get; }

		/// <summary>
		/// Gets the depth stencil state for rendering with this material.
		/// </summary>
		DepthStencilState DepthStencilState { get; }

		/// <summary>
		/// Gets the rasterizer state for rendering with this material.
		/// </summary>
		RasterizerState RasterizerState { get; }

		/// <summary>
		/// Gets the effect binding for shadow rendering with this material.
		/// </summary>
		/// <param name="materialTechnique">The material technique to use.</param>
		/// <returns>The effect binding for shadow rendering.</returns>
		EffectBinding GetShadowTechnique(MaterialTechnique materialTechnique);

		/// <summary>
		/// Gets the effect binding for color rendering with this material.
		/// </summary>
		/// <param name="materialTechnique">The material technique to use.</param>
		/// <param name="lightTechnique">The type of lighting to apply.</param>
		/// <param name="shadow">Whether shadows should be rendered.</param>
		/// <param name="translucent">Whether the material is rendered as translucent.</param>
		/// <param name="normalMapping">Whether normal mapping should be applied.</param>
		/// <param name="clipPlane">Whether clip plane is in use.</param>
		/// <returns>The effect binding for color rendering.</returns>
		EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane);

		/// <summary>
		/// Creates a copy of this material instance.
		/// </summary>
		/// <returns>A cloned instance of this material.</returns>
		IMaterial Clone();
	}
}
