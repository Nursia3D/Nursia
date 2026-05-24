using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.SceneGraph.Landscape
{
	/// <summary>
	/// Material for terrain rendering with support for lighting and shadows.
	/// </summary>
	public class TerrainMaterial : BaseMaterial, IHasExternalAssets
	{
		private static readonly EffectBinding[] _allBindings = new EffectBinding[128];

		private MaterialFlags _flags = MaterialFlags.AcceptsLight | MaterialFlags.CastsShadows | MaterialFlags.AcceptsShadows;

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
		/// Gets or sets the ambient color of the terrain.
		/// </summary>
		[Category("Appearance")]
		public Color AmbientColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the diffuse color of the terrain.
		/// </summary>
		[Category("Appearance")]
		public Color DiffuseColor { get; set; } = Color.White;

		/// <summary>
		/// Gets or sets the specular color for reflections on the terrain.
		/// </summary>
		[Category("Appearance")]
		public Color SpecularColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the power for specular highlights.
		/// </summary>
		[Category("Appearance")]
		public float SpecularPower { get; set; } = 250.0f;

		/// <summary>
		/// Gets or sets the emissive color of the terrain.
		/// </summary>
		[Category("Appearance")]
		public Color EmissiveColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the tiling factor for detail maps.
		/// </summary>
		[Category("Appearance")]
		public Vector2 DetailTiling { get; set; } = new Vector2(32, 32);

		/// <summary>
		/// Gets or sets the weight map texture used to blend detail layers.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D WeightMap1 { get; set; }

		/// <summary>
		/// Gets a string representation of the weight map dimensions.
		/// </summary>
		[JsonIgnore]
		public string WeightMap1Size => WeightMap1 != null ? $"{WeightMap1.Width}x{WeightMap1.Height}" : "null";

		/// <summary>
		/// Gets or sets the path to the weight map asset.
		/// </summary>
		[Category("Appearance")]
		[Browsable(false)]
		public string WeightMap1Path { get; set; }

		/// <summary>
		/// Gets or sets the first detail map texture.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DetailMap1 { get; set; }

		/// <summary>
		/// Gets or sets the path to the first detail map asset.
		/// </summary>
		[Category("Appearance")]
		[Browsable(false)]
		public string DetailMap1Path { get; set; }

		/// <summary>
		/// Gets or sets the second detail map texture.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DetailMap2 { get; set; }

		/// <summary>
		/// Gets or sets the path to the second detail map asset.
		/// </summary>
		[Category("Appearance")]
		[Browsable(false)]
		public string DetailMap2Path { get; set; }

		/// <summary>
		/// Gets or sets the third detail map texture.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DetailMap3 { get; set; }

		/// <summary>
		/// Gets or sets the path to the third detail map asset.
		/// </summary>
		[Category("Appearance")]
		[Browsable(false)]
		public string DetailMap3Path { get; set; }

		/// <summary>
		/// Gets or sets the fourth detail map texture.
		/// </summary>
		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DetailMap4 { get; set; }

		/// <summary>
		/// Gets or sets the path to the fourth detail map asset.
		/// </summary>
		[Category("Appearance")]
		[Browsable(false)]
		public string DetailMap4Path { get; set; }

		/// <summary>
		/// Creates a copy of this terrain material.
		/// </summary>
		/// <returns>A new TerrainMaterial with the same properties as this material.</returns>
		public override IMaterial Clone()
		{
			var result = new TerrainMaterial
			{
				CastsShadows = CastsShadows,
				AcceptsShadows = AcceptsShadows,
				AmbientColor = AmbientColor,
				DiffuseColor = DiffuseColor,
				SpecularColor = SpecularColor,
				EmissiveColor = EmissiveColor,
				DetailTiling = DetailTiling,
				WeightMap1 = WeightMap1,
				WeightMap1Path = WeightMap1Path,
				DetailMap1 = DetailMap1,
				DetailMap1Path = DetailMap1Path,
				DetailMap2 = DetailMap2,
				DetailMap2Path = DetailMap2Path,
				DetailMap3 = DetailMap3,
				DetailMap3Path = DetailMap3Path,
				DetailMap4 = DetailMap4,
				DetailMap4Path = DetailMap4Path
			};

			return result;
		}

		private static EffectBinding InternalGetBinding(LightTechnique lightTechnique, bool shadow, bool translucent, bool marker, bool clipPlane)
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

			if (shadow)
			{
				if (Nrs.GraphicsSettings.ShadowType == ShadowType.Simple)
				{
					key |= 4;
				}

				if (Nrs.GraphicsSettings.ShadowType == ShadowType.PCF)
				{
					key |= 8;
				}
			}

			if (translucent)
			{
				key |= 16;
			}

			if (marker)
			{
				key |= 32;
			}

			if (clipPlane)
			{
				key |= 64;
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

			if (marker)
			{
				defines["MARKER"] = "1";
			}

			if (clipPlane)
			{
				defines["CLIPPLANE"] = "1";
			}

			binding = EffectsRegistry.GetStockEffectBinding("TerrainBlend", defines);

			binding.AddMaterialLevelSetter<TerrainMaterial>("cMatAmbientColor", (m, p) => p.SetValue(m.AmbientColor.ToVector3()));
			binding.AddMaterialLevelSetter<TerrainMaterial>("cMatDiffColor", (m, p) => p.SetValue(m.DiffuseColor.ToVector4()));
			binding.AddMaterialLevelSetter<TerrainMaterial>("cMatSpecColor", (m, p) => p.SetValue(m.SpecularColor.ToVector4()));
			binding.AddMaterialLevelSetter<TerrainMaterial>("cMatSpecularPower", (m, p) => p.SetValue(m.SpecularPower));
			binding.AddMaterialLevelSetter<TerrainMaterial>("cMatEmissiveColor", (m, p) => p.SetValue(m.EmissiveColor.ToVector3()));

			binding.AddMaterialLevelSetter<TerrainMaterial>("cDetailTiling", (m, p) => p.SetValue(m.DetailTiling));

			binding.AddMaterialLevelSetter<TerrainMaterial>("WeightMap0", (m, p) => p.SetValue(m.WeightMap1 ?? Resources.White));
			binding.AddMaterialLevelSetter<TerrainMaterial>("DetailMap1", (m, p) => p.SetValue(m.DetailMap1 ?? Resources.White));
			binding.AddMaterialLevelSetter<TerrainMaterial>("DetailMap2", (m, p) => p.SetValue(m.DetailMap2 ?? Resources.White));
			binding.AddMaterialLevelSetter<TerrainMaterial>("DetailMap3", (m, p) => p.SetValue(m.DetailMap3 ?? Resources.White));
			binding.AddMaterialLevelSetter<TerrainMaterial>("DetailMap4", (m, p) => p.SetValue(m.DetailMap4 ?? Resources.White));

			binding.AddMaterialLevelSetter<TerrainMaterial>("MarkerPosition", (m, p) => p.SetValue(Nrs.EditorSettings.TerrainMarkerPosition.Value));
			binding.AddMaterialLevelSetter<TerrainMaterial>("MarkerRadius", (m, p) => p.SetValue(Nrs.EditorSettings.TerrainMarkerRadius));

			_allBindings[key] = binding;

			return binding;
		}

		/// <summary>
		/// Gets the appropriate shader technique for rendering this material under the specified conditions.
		/// </summary>
		/// <param name="materialTechnique">The material rendering technique.</param>
		/// <param name="lightTechnique">The light type technique.</param>
		/// <param name="shadow">Whether shadows are enabled.</param>
		/// <param name="translucent">Whether the material is translucent.</param>
		/// <param name="normalMapping">Whether normal mapping is enabled.</param>
		/// <param name="clipPlane">Whether clip plane is enabled.</param>
		/// <returns>The effect binding for the specified technique.</returns>
		public override EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane)
		{
			return InternalGetBinding(lightTechnique, shadow, translucent, Nrs.EditorSettings.TerrainMarkerPosition != null, clipPlane);
		}

		/// <summary>
		/// Loads external texture assets for this material.
		/// </summary>
		/// <param name="assetManager">The asset manager to load resources from.</param>
		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(WeightMap1Path))
			{
				WeightMap1 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, WeightMap1Path);
			}

			if (!string.IsNullOrEmpty(DetailMap1Path))
			{
				DetailMap1 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, DetailMap1Path);
			}

			if (!string.IsNullOrEmpty(DetailMap2Path))
			{
				DetailMap2 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, DetailMap2Path);
			}

			if (!string.IsNullOrEmpty(DetailMap3Path))
			{
				DetailMap3 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, DetailMap3Path);
			}

			if (!string.IsNullOrEmpty(DetailMap4Path))
			{
				DetailMap4 = assetManager.LoadTexture2D(Nrs.GraphicsDevice, DetailMap4Path);
			}
		}
	}
}
