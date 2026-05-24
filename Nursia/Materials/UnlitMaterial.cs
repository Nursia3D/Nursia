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
	/// <summary>
	/// Unlit material that renders without any lighting calculations.
	/// </summary>
	public class UnlitMaterial : BaseMaterial, IHasExternalAssets
	{
		private static readonly EffectBinding[] _allBindings = new EffectBinding[8];

		private MaterialFlags _flags = MaterialFlags.CastsShadows;

		/// <summary>
		/// Gets or sets the unique identifier for this material.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets the material flags describing this material's properties.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
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
		/// Gets or sets a value indicating whether this material is transparent.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the texture for this unlit material.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D Texture { get; set; }

		/// <summary>
		/// Gets or sets the path to the texture asset.
		/// </summary>
		[Category("Appearance")]
		[Browsable(false)]
		public string TexturePath { get; set; }

		/// <summary>
		/// Gets or sets the diffuse color of the material.
		/// </summary>
		[Category("Appearance")]
		public Color DiffuseColor { get; set; } = Color.White;

		/// <summary>
		/// Loads the external texture referenced by this material using the specified asset manager.
		/// </summary>
		/// <param name="assetManager">The asset manager to load the texture from.</param>
		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(TexturePath))
			{
				Texture = assetManager.LoadTexture2D(Nrs.GraphicsDevice, TexturePath);
			}
		}

		/// <summary>
		/// Creates a copy of this material instance.
		/// </summary>
		/// <returns>A cloned instance of this material with identical properties.</returns>
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

		/// <summary>
		/// Gets the effect binding for rendering color with this unlit material.
		/// </summary>
		/// <param name="materialTechnique">The material technique to use.</param>
		/// <param name="lightTechnique">The type of lighting to apply (ignored for unlit material).</param>
		/// <param name="shadow">Whether shadows should be rendered (ignored for unlit material).</param>
		/// <param name="translucent">Whether the material is rendered as translucent (ignored for unlit material).</param>
		/// <param name="normalMapping">Whether normal mapping should be applied (ignored for unlit material).</param>
		/// <param name="clipPlane">Whether clip plane is in use.</param>
		/// <returns>The effect binding for unlit color rendering.</returns>
		public override EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane)
		{
			return InternalGetBinding(materialTechnique, Texture != null);
		}
	}
}
