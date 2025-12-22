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
	public class UnlitMaterial : BaseMaterial, IHasExternalAssets
	{
		private static readonly EffectBinding[] _allBindings = new EffectBinding[8];

		private MaterialFlags _flags = MaterialFlags.CastsShadows;

		public string Id { get; set; }

		[Browsable(false)]
		[JsonIgnore]
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
		[DefaultValue(false)]
		public bool IsTransparent
		{
			get => _flags.HasFlag(MaterialFlags.IsTransparent);

			set
			{
				if (value)
				{
					_flags |= MaterialFlags.IsTransparent;
				}
				else
				{
					_flags &= ~MaterialFlags.IsTransparent;
				}
			}
		}

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D Texture { get; set; }

		[Category("Appearance")]
		[Browsable(false)]
		public string TexturePath { get; set; }

		[Category("Appearance")]
		public Color DiffuseColor { get; set; } = Color.White;

		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(TexturePath))
			{
				Texture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, TexturePath);
			}
		}

		public override IMaterial Clone()
		{
			return new UnlitMaterial
			{
				Id = Id,
				BlendState = BlendState,
				DepthStencilState = DepthStencilState,
				CastsShadows = CastsShadows,
				IsTransparent = IsTransparent,
				Texture = Texture,
				TexturePath = TexturePath,
				DiffuseColor = DiffuseColor,
			};
		}

		private static EffectBinding InternalGetBinding(MaterialTechnique materialTechnique, bool texture)
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

			if (texture)
			{
				key |= 4;
			}

			if (_allBindings[key] != null)
			{
				return _allBindings[key];
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

			if (texture)
			{
				defines["DIFFMAP"] = "1";
			}

			var binding = EffectsRegistry.GetStockEffectBinding("Unlit", defines);

			binding.AddMaterialLevelSetter<UnlitMaterial>("cMatDiffColor", (m, p) => p.SetValue(m.DiffuseColor.ToVector4()));
			binding.AddMaterialLevelSetter<UnlitMaterial>("DiffMap", (m, p) => p.SetValue(m.Texture));

			_allBindings[key] = binding;

			return binding;
		}

		public override EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane)
		{
			return InternalGetBinding(materialTechnique, Texture != null);
		}
	}
}
