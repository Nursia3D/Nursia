using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Serialization;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Nursia.Materials
{
	/// <summary>
	/// Blinn-Phong material implementation for realistic lighting with optional texture mapping.
	/// </summary>
	public class BlinnPhongMaterial : BaseMaterial, IHasExternalAssets
	{
		private static readonly EffectBinding[] _allBindings = new EffectBinding[512];

		private MaterialFlags _flags = MaterialFlags.AcceptsLight | MaterialFlags.CastsShadows | MaterialFlags.AcceptsShadows;

		/// <summary>
		/// Gets or sets the unique identifier for this material.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets the material flags describing this material's properties.
		/// </summary>
		public override MaterialFlags Flags => _flags;

		/// <summary>
		/// Gets or sets a value indicating whether this material casts shadows.
		/// </summary>
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

		/// <summary>
		/// Gets or sets a value indicating whether this material accepts shadows.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the ambient color of the material.
		/// </summary>
		[Category("Appearance")]
		public Color AmbientColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the diffuse color of the material.
		/// </summary>
		[Category("Appearance")]
		public Color DiffuseColor { get; set; } = Color.White;

		/// <summary>
		/// Gets or sets the specular color of the material.
		/// </summary>
		[Category("Appearance")]
		public Color SpecularColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the specular power (shininess) of the material.
		/// </summary>
		[Category("Appearance")]
		public float SpecularPower { get; set; } = 250.0f;

		/// <summary>
		/// Gets or sets the emissive color of the material.
		/// </summary>
		[Category("Appearance")]
		public Color EmissiveColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the diffuse texture of the material.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DiffuseTexture { get; set; }

		/// <summary>
		/// Gets or sets the path to the diffuse texture asset.
		/// </summary>
		[Browsable(false)]
		public string DiffuseTexturePath { get; set; }

		/// <summary>
		/// Gets or sets the specular texture of the material.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D SpecularTexture { get; set; }

		/// <summary>
		/// Gets or sets the path to the specular texture asset.
		/// </summary>
		[Browsable(false)]
		public string SpecularTexturePath { get; set; }

		/// <summary>
		/// Gets or sets the normal map texture of the material.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D NormalTexture { get; set; }

		/// <summary>
		/// Gets or sets the path to the normal map texture asset.
		/// </summary>
		[Browsable(false)]
		public string NormalTexturePath { get; set; }

		/// <summary>
		/// Loads all external textures referenced by this material using the specified asset manager.
		/// </summary>
		/// <param name="assetManager">The asset manager to load textures from.</param>
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

		/// <summary>
		/// Creates a copy of this material instance.
		/// </summary>
		/// <returns>A cloned instance of this material with identical properties.</returns>
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

		/// <summary>
		/// Gets the effect binding for rendering color with this material.
		/// </summary>
		/// <param name="materialTechnique">The material technique to use.</param>
		/// <param name="lightTechnique">The type of lighting to apply.</param>
		/// <param name="shadow">Whether shadows should be rendered.</param>
		/// <param name="translucent">Whether the material is rendered as translucent.</param>
		/// <param name="normalMapping">Whether normal mapping should be applied.</param>
		/// <param name="clipPlane">Whether clip plane is in use.</param>
		/// <returns>The effect binding for color rendering.</returns>
		public override EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane)
		{
			return InternalGetBinding(materialTechnique, lightTechnique, shadow, translucent, normalMapping && NormalTexture != null, clipPlane);
		}

		private static string GetRelativePath(string path, string modelFolder)
		{
			if (!string.IsNullOrEmpty(modelFolder))
			{
				path = Path.Combine(modelFolder, path);
			}

			return path.NormalizeFilePath();
		}

		internal static BlinnPhongMaterial FromDrMaterial(DrMaterial source, string modelFolder)
		{
			var result = new BlinnPhongMaterial
			{
				DiffuseColor = source.DiffuseColor,
				SpecularColor = source.SpecularColor,
				SpecularPower = source.Shininess,
				EmissiveColor = source.EmissiveColor,
				DiffuseTexture = source.DiffuseTexture,
				SpecularTexture = source.SpecularTexture,
				NormalTexture = source.NormalTexture,
			};

			if (source.DiffuseTexture != null)
			{
				result.DiffuseTexturePath = GetRelativePath(source.DiffuseTexture.Name, modelFolder);
			}

			if (source.SpecularTexture != null)
			{
				result.SpecularTexturePath = GetRelativePath(source.SpecularTexture.Name, modelFolder);
			}

			if (source.NormalTexture != null)
			{
				result.NormalTexturePath = GetRelativePath(source.NormalTexture.Name, modelFolder);
			}

			return result;
		}

	}
}