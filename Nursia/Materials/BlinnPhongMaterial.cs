using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.Materials
{
	public class BlinnPhongMaterial : BaseMaterial, IHasExternalAssets
	{
		private static readonly EffectBinding[] _allBindings = new EffectBinding[512];

		private MaterialFlags _flags = MaterialFlags.AcceptsLight | MaterialFlags.CastsShadows | MaterialFlags.AcceptsShadows;

		public string Id { get; set; }

		public override MaterialFlags Flags => _flags;

		[Category("Behavior")]
		[DefaultValue(true)]
		public bool CastsShadows
		{
			get => _flags.HasFlag(MaterialFlags.CastsShadows);

			set
			{
				if (value)
				{
					_flags |= MaterialFlags.CastsShadows;
				}
				else
				{
					_flags &= ~MaterialFlags.CastsShadows;
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		public bool AcceptsShadows
		{
			get => _flags.HasFlag(MaterialFlags.AcceptsShadows);

			set
			{
				if (value)
				{
					_flags |= MaterialFlags.AcceptsShadows;
				}
				else
				{
					_flags &= ~MaterialFlags.AcceptsShadows;
				}
			}
		}

		[Category("Appearance")]
		public Color AmbientColor { get; set; } = Color.Black;

		[Category("Appearance")]
		public Color DiffuseColor { get; set; } = Color.White;

		[Category("Appearance")]
		public Color SpecularColor { get; set; } = Color.Black;

		[Category("Appearance")]
		public float SpecularPower { get; set; } = 250.0f;

		[Category("Appearance")]
		public Color EmissiveColor { get; set; } = Color.Black;

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DiffuseTexture { get; set; }

		[Browsable(false)]
		public string DiffuseTexturePath { get; set; }

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D SpecularTexture { get; set; }

		[Browsable(false)]
		public string SpecularTexturePath { get; set; }

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D NormalTexture { get; set; }

		[Browsable(false)]
		public string NormalTexturePath { get; set; }

		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(DiffuseTexturePath))
			{
				DiffuseTexture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, DiffuseTexturePath);
			}

			if (!string.IsNullOrEmpty(SpecularTexturePath))
			{
				SpecularTexture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, SpecularTexturePath);
			}

			if (!string.IsNullOrEmpty(NormalTexturePath))
			{
				NormalTexture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, NormalTexturePath);
			}
		}

		public override IMaterial Clone()
		{
			return new BlinnPhongMaterial
			{
				Id = Id,
				BlendState = BlendState,
				DepthStencilState = DepthStencilState,
				RasterizerState = RasterizerState,
				CastsShadows = CastsShadows,
				AcceptsShadows = AcceptsShadows,
				AmbientColor = AmbientColor,
				DiffuseColor = DiffuseColor,
				SpecularColor = SpecularColor,
				EmissiveColor = EmissiveColor,
				DiffuseTexture = DiffuseTexture,
				DiffuseTexturePath = DiffuseTexturePath,
				SpecularTexture = SpecularTexture,
				SpecularTexturePath = SpecularTexturePath,
				NormalTexture = NormalTexture,
				NormalTexturePath = NormalTexturePath
			};
		}

		private static EffectBinding InternalGetBinding(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMap, bool clipPlane)
		{
			var key = 0;

			switch (materialTechnique)
			{
				case MaterialTechnique.Skinned:
					key |= 1;
					break;
				case MaterialTechnique.Instanced:
					key |= 2;
					break;
			}

			switch (lightTechnique)
			{
				case LightTechnique.Point:
					key |= 4;
					break;
				case LightTechnique.Spot:
					key |= 8;
					break;
			}

			if (shadow)
			{
				if (Nrs.GraphicsSettings.ShadowType == ShadowType.Simple)
				{
					key |= 16;
				}

				if (Nrs.GraphicsSettings.ShadowType == ShadowType.PCF)
				{
					key |= 32;
				}
			}

			if (translucent)
			{
				key |= 64;
			}

			if (normalMap)
			{
				key |= 128;
			}

			if (clipPlane)
			{
				key |= 256;
			}

			var binding = _allBindings[key];
			if (binding != null)
			{
				return binding;
			}

			var defines = new Dictionary<string, string>();

			switch (materialTechnique)
			{
				case MaterialTechnique.Skinned:
					defines["SKINNED"] = "1";
					break;
				case MaterialTechnique.Instanced:
					defines["INSTANCED"] = "1";
					break;
			}

			switch (lightTechnique)
			{
				case LightTechnique.Point:
					defines["POINTLIGHT"] = "1";
					break;
				case LightTechnique.Spot:
					defines["SPOTLIGHT"] = "1";
					break;
				default:
					defines["DIRLIGHT"] = "1";
					break;
			}

			if (shadow)
			{
				defines["SHADOW"] = "1";

				if (Nrs.GraphicsSettings.ShadowType == ShadowType.PCF)
				{
					defines["PCFSHADOW"] = "1";
				}
				else if (Nrs.GraphicsSettings.ShadowType == ShadowType.Simple)
				{
					defines["SIMPLESHADOW"] = "1";
				}
			}

			if (translucent)
			{
				defines["TRANSLUCENT"] = "1";
			}

			if (normalMap)
			{
				defines["NORMALMAP"] = "1";
			}

			if (clipPlane)
			{
				defines["CLIPPLANE"] = "1";
			}

			binding = EffectsRegistry.GetStockEffectBinding("BlinnPhong", defines);

			binding.AddMaterialLevelSetter<BlinnPhongMaterial>("cMatAmbientColor", (m, p) => p.SetValue(m.AmbientColor.ToVector3()));
			binding.AddMaterialLevelSetter<BlinnPhongMaterial>("cMatDiffColor", (m, p) => p.SetValue(m.DiffuseColor.ToVector4()));
			binding.AddMaterialLevelSetter<BlinnPhongMaterial>("cMatSpecColor", (m, p) => p.SetValue(m.SpecularColor.ToVector4()));
			binding.AddMaterialLevelSetter<BlinnPhongMaterial>("cMatSpecularPower", (m, p) => p.SetValue(m.SpecularPower));
			binding.AddMaterialLevelSetter<BlinnPhongMaterial>("cMatEmissiveColor", (m, p) => p.SetValue(m.EmissiveColor.ToVector3()));

			binding.AddMaterialLevelSetter<BlinnPhongMaterial>("DiffMap", (m, p) => p.SetValue(m.DiffuseTexture ?? Resources.White));
			binding.AddMaterialLevelSetter<BlinnPhongMaterial>("SpecMap", (m, p) => p.SetValue(m.SpecularTexture ?? Resources.White));
			binding.AddMaterialLevelSetter<BlinnPhongMaterial>("NormalMap", (m, p) => p.SetValue(m.NormalTexture ?? Resources.White));

			_allBindings[key] = binding;

			return binding;
		}

		public override EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane)
		{
			return InternalGetBinding(materialTechnique, lightTechnique, shadow, translucent, normalMapping && NormalTexture != null, clipPlane);
		}
	}
}