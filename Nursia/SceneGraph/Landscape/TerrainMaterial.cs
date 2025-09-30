using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.SceneGraph.Lights;
using Nursia.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.SceneGraph.Landscape
{
	public class TerrainMaterial : IMaterial, IHasExternalAssets
	{
		private static readonly EffectBinding[] _allBindings = new EffectBinding[128];

		private MaterialFlags _flags = MaterialFlags.AcceptsLight | MaterialFlags.CastsShadows | MaterialFlags.AcceptsShadows;

		[Browsable(false)]
		[JsonIgnore]
		public BlendState BlendState => null;

		[Browsable(false)]
		[JsonIgnore]
		public DepthStencilState DepthStencilState => null;
		
		[Browsable(false)]
		[JsonIgnore]
		public RasterizerState RasterizerState => null;

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
		public Vector2 DetailTiling { get; set; } = new Vector2(32, 32);

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D WeightMap1 { get; set; }

		[JsonIgnore]
		public string WeightMap1Size => WeightMap1 != null ? $"{WeightMap1.Width}x{WeightMap1.Height}" : "null";

		[Category("Appearance")]
		[Browsable(false)]
		public string WeightMap1Path { get; set; }

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DetailMap1 { get; set; }

		[Category("Appearance")]
		[Browsable(false)]
		public string DetailMap1Path { get; set; }

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DetailMap2 { get; set; }

		[Category("Appearance")]
		[Browsable(false)]
		public string DetailMap2Path { get; set; }

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DetailMap3 { get; set; }

		[Category("Appearance")]
		[Browsable(false)]
		public string DetailMap3Path { get; set; }

		[Category("Appearance")]
		[JsonIgnore]
		public Texture2D DetailMap4 { get; set; }

		[Category("Appearance")]
		[Browsable(false)]
		public string DetailMap4Path { get; set; }

		public IMaterial Clone()
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

		private static EffectBinding InternalGetBinding(LightTechnique lightTechnique, ShadowType shadow, bool translucent, bool marker, bool clipPlane)
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

		public EffectBinding GetEffectBinding(LightTechnique technique, ShadowType shadow, bool translucent, DrMeshPart mesh, bool clipPlane)
		{
			return InternalGetBinding(technique, shadow, translucent, Nrs.EditorSettings.TerrainMarkerPosition != null, clipPlane);
		}

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
