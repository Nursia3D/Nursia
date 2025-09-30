using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.SceneGraph.Lights;
using Nursia.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.Materials
{
	public class BlinnPhongMaterial : IMaterial, IHasExternalAssets
	{
		private static readonly EffectBinding[] _allBindings = new EffectBinding[256];

		private MaterialFlags _flags = MaterialFlags.AcceptsLight | MaterialFlags.CastsShadows | MaterialFlags.AcceptsShadows;

		public string Id { get; set; }

		[Category("States")]
		public BlendState BlendState { get; set; }

		[Category("States")]
		public DepthStencilState DepthStencilState { get; set; }

		[Category("States")]
		public RasterizerState RasterizerState { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public MaterialFlags Flags => _flags;

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

		public IMaterial Clone()
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

		private static EffectBinding InternalGetBinding(LightTechnique lightTechnique, ShadowType shadow,
			bool translucent, bool skinning, bool normalMap, bool clipPlane)
		{
			var key = 0;

			if (lightTechnique == LightTechnique.Point)
			{
				key |= 1;
			}

			if (lightTechnique == LightTechnique.Spot)
			{
				key |= 2;
			}

			if (shadow == ShadowType.Simple)
			{
				key |= 4;
			}

			if (shadow == ShadowType.PCF)
			{
				key |= 8;
			}

			if (translucent)
			{
				key |= 16;
			}

			if (skinning)
			{
				key |= 32;
			}

			if (normalMap)
			{
				key |= 64;
			}

			if (clipPlane)
			{
				key |= 128;
			}

			var binding = _allBindings[key];
			if (binding != null)
			{
				return binding;
			}

			var defines = new Dictionary<string, string>();

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

			if (shadow != ShadowType.None)
			{
				defines["SHADOW"] = "1";

				if (shadow == ShadowType.PCF)
				{
					defines["PCFSHADOW"] = "1";
				}
				else if (shadow == ShadowType.Simple)
				{
					defines["SIMPLESHADOW"] = "1";
				}
			}

			if (translucent)
			{
				defines["TRANSLUCENT"] = "1";
			}

			if (skinning)
			{
				defines["SKINNED"] = "1";
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

		public EffectBinding GetEffectBinding(LightTechnique technique, ShadowType shadow, bool translucent, DrMeshPart mesh, bool clipPlane)
		{
			return InternalGetBinding(technique, shadow, translucent, mesh != null && mesh.Skin != null,
				!Nrs.DebugSettings.DisableNormalMap && NormalTexture != null && mesh != null && mesh.TangentsFormat == VertexElementFormat.Vector4,
				clipPlane);
		}
	}
}