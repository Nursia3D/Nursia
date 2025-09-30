using DigitalRiseModel;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.SceneGraph.Lights;
using System;
using System.ComponentModel;

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
		BlendState BlendState { get; }

		DepthStencilState DepthStencilState { get; }

		RasterizerState RasterizerState { get; }

		MaterialFlags Flags { get; }

		EffectBinding GetEffectBinding(LightTechnique technique, ShadowType shadow, bool translucent, DrMeshPart mesh, bool clipPlane);
		IMaterial Clone();
	}
}
