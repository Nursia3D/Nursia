using DigitalRiseModel;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.SceneGraph.Lights;
using System;

namespace Nursia.Materials
{
	public enum LightTechnique
	{
		Direct,
		Point,
		Spot
	}

	[Flags]
	public enum MaterialFlags
	{
		None = 0,
		AcceptsDirectionalLight = 1 << 0,
		AcceptsPointLight = 1 << 1,
		AcceptsSpotLight = 1 << 2,
		CastsShadows = 1 << 3,
		AcceptsShadows = 1 << 4,
		IsTransparent = 1 << 5,
		RequiresDepthBuffer = 1 << 6,
		RequiresScreenTexture = 1 << 7,

		AcceptsLight = AcceptsDirectionalLight | AcceptsPointLight | AcceptsSpotLight
	}

	public interface IMaterial
	{
		MaterialFlags Flags { get; }

		BlendState BlendState { get; }

		DepthStencilState DepthStencilState { get; }

		RasterizerState RasterizerState { get; }

		EffectBinding GetShadowTechnique(DrMeshPart mesh);

		EffectBinding GetColorTechnique(LightTechnique technique, bool shadow, bool translucent, DrMeshPart mesh, bool clipPlane);
		
		IMaterial Clone();
	}
}
