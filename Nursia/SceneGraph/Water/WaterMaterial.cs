using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Materials;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.SceneGraph.Water
{
	public class WaterMaterial : IMaterial
	{
		private static EffectBinding _binding;


		[Browsable(false)]
		[JsonIgnore]
		public BlendState BlendState => BlendState.NonPremultiplied;

		[Browsable(false)]
		[JsonIgnore]
		public DepthStencilState DepthStencilState => null;

		[Browsable(false)]
		[JsonIgnore]
		public RasterizerState RasterizerState => null;

		[Browsable(false)]
		[JsonIgnore]
		public MaterialFlags Flags => MaterialFlags.RequiresDepthBuffer | MaterialFlags.RequiresScreenTexture;

		[Category("Behavior")]
		public Vector2 NoiseSpeed { get; set; } = new Vector2(0.05f, 0.05f);

		[Category("Behavior")]
		[DefaultValue(1.0f)]
		public float NoiseTiling { get; set; } = 1.0f;

		[Category("Behavior")]
		[DefaultValue(0.02f)]
		public float NoiseStrength { get; set; } = 0.02f;

		[Category("Behavior")]
		[DefaultValue(8.0f)]
		public float FresnelPower { get; set; } = 8.0f;

		[Category("Behavior")]
		[DefaultValue(0.0f)]
		public float MurkinessStart { get; set; } = 0.0f;

		[Category("Behavior")]
		[DefaultValue(0.0f)]
		public float MurkinessFactor { get; set; } = 0.0f;

		[Category("Behavior")]
		[DefaultValue(1.0f)]
		public float EdgeFactor { get; set; } = 1.0f;

		[Category("Appearance")]
		public Color WaterTint { get; set; } = new Color(0.8f, 0.8f, 1.0f);

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D NormalTexture { get; set; }

		[Category("Appearance")]
		[Browsable(false)]
		public string NormalTexturePath { get; set; }



		public IMaterial Clone()
		{
			return new WaterMaterial
			{
				NoiseSpeed = NoiseSpeed,
				NoiseTiling = NoiseTiling,
				NoiseStrength = NoiseStrength,
				FresnelPower = FresnelPower,
				WaterTint = WaterTint,
				MurkinessStart = MurkinessStart,
				MurkinessFactor = MurkinessFactor,
				EdgeFactor = EdgeFactor,
				NormalTexture = NormalTexture,
				NormalTexturePath = NormalTexturePath
			};
		}

		private static EffectBinding InternalGetBinding()
		{
			if (_binding != null)
			{
				return _binding;
			}

			_binding = EffectsRegistry.GetStockEffectBinding("Water");

			_binding.AddMaterialLevelSetter<WaterMaterial>("cNoiseSpeed", (m, p) => p.SetValue(m.NoiseSpeed));
			_binding.AddMaterialLevelSetter<WaterMaterial>("cNoiseTiling", (m, p) => p.SetValue(m.NoiseTiling));
			_binding.AddMaterialLevelSetter<WaterMaterial>("cNoiseStrength", (m, p) => p.SetValue(m.NoiseStrength));
			_binding.AddMaterialLevelSetter<WaterMaterial>("cFresnelPower", (m, p) => p.SetValue(m.FresnelPower));
			_binding.AddMaterialLevelSetter<WaterMaterial>("cWaterTint", (m, p) => p.SetValue(m.WaterTint.ToVector3()));
			_binding.AddMaterialLevelSetter<WaterMaterial>("cMurkinessStart", (m, p) => p.SetValue(m.MurkinessStart));
			_binding.AddMaterialLevelSetter<WaterMaterial>("cMurkinessFactor", (m, p) => p.SetValue(m.MurkinessFactor));
			_binding.AddMaterialLevelSetter<WaterMaterial>("cEdgeFactor", (m, p) => p.SetValue(m.EdgeFactor));

			_binding.AddMaterialLevelSetter<WaterMaterial>("NormalMap", (m, p) => p.SetValue(m.NormalTexture ?? Resources.WaterNormals));

			return _binding;
		}

		public EffectBinding GetShadowTechnique(DrMeshPart mesh, bool instancing)
		{
			throw new System.NotImplementedException();
		}

		public EffectBinding GetColorTechnique(DrMeshPart mesh, LightTechnique technique, bool shadow, bool translucent, bool clipPlane, bool instancing)
		{
			return InternalGetBinding();
		}
	}
}
