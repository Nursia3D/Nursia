using AssetManagementBase;
using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.Serialization;
using System;
using System.ComponentModel;

namespace Nursia.Env.Sky
{
	public class Skybox
	{
		private class SkyboxMaterial : IMaterial, IHasExternalAssets
		{
			private static EffectBinding _binding;

			public BlendState BlendState => BlendState.Opaque;

			public DepthStencilState DepthStencilState => null;
			public RasterizerState RasterizerState => RasterizerState.CullNone;
			public MaterialFlags Flags => MaterialFlags.None;

			public Color DiffuseColor { get; set; } = Color.White;
			public Matrix Transform { get; set; } = Matrix.Identity;

			[JsonIgnore]
			public TextureCube DiffuseTexture { get; set; }

			[Browsable(false)]
			public string DiffuseTexturePath { get; set; }

			public IMaterial Clone()
			{
				throw new NotImplementedException();
			}

			public EffectBinding GetShadowTechnique(DrMeshPart mesh, bool instancing)
			{
				throw new NotImplementedException();
			}

			public EffectBinding GetColorTechnique(DrMeshPart mesh, LightTechnique technique, bool shadow, bool translucent, bool clipPlane, bool instancing)
			{
				if (_binding == null)
				{
					_binding = EffectsRegistry.GetStockEffectBinding("Skybox");

					_binding.AddMaterialLevelSetter<SkyboxMaterial>("cMatDiffColor", (m, p) => p.SetValue(m.DiffuseColor.ToVector4()));
					_binding.AddMaterialLevelSetter<SkyboxMaterial>("DiffCubeMap", (m, p) => p.SetValue(m.DiffuseTexture));
					_binding.AddMaterialLevelSetter<SkyboxMaterial>("cCustomTransform", (m, p) => p.SetValue(m.Transform));
				}

				return _binding;
			}


			public void Load(AssetManager assetManager)
			{
				if (!string.IsNullOrEmpty(DiffuseTexturePath))
				{
					DiffuseTexture = assetManager.LoadTextureCube(Nrs.GraphicsDevice, DiffuseTexturePath);
				}
			}
		}

		private readonly DrMeshPart _mesh;
		private readonly SkyboxMaterial _material = new SkyboxMaterial();

		public Color DiffuseColor
		{
			get => _material.DiffuseColor;

			set => _material.DiffuseColor = value;
		}

		[DefaultValue(true)]
		public bool Visible { get; set; } = true;

		[JsonIgnore]
		public TextureCube DiffuseTexture
		{
			get => _material.DiffuseTexture;

			set => _material.DiffuseTexture = value;
		}

		[Browsable(false)]
		public string DiffuseTexturePath
		{
			get => _material.DiffuseTexturePath;

			set => _material.DiffuseTexturePath = value;
		}


		[JsonIgnore]
		[Browsable(false)]
		public Matrix LocalTransform { get; set; } = Matrix.CreateScale(100.0f);

		public Skybox()
		{
			_mesh = MeshPrimitives.CreateBoxMeshPart(Nrs.GraphicsDevice, Vector3.One);
		}

		public Skybox(DrMeshPart mesh)
		{
			_mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
		}

		public void Render(IRenderBatch batch)
		{
			if (!Visible || _material.DiffuseTexture == null)
			{
				return;
			}

			// Calculate special world-view-project matrix with zero translation
			// So the viewer is always in the center
			var view = batch.Camera.View;
			view.Translation = Vector3.Zero;

			_material.Transform = LocalTransform * view * batch.Camera.Projection;

			batch.BatchJob(_material, Matrix.Identity, _mesh, cullByBoundingBox: false);
		}

		public void Load(AssetManager assetManager)
		{
			_material.Load(assetManager);
		}

		public Skybox Clone()
		{
			return new Skybox
			{
				DiffuseColor = DiffuseColor,
				DiffuseTexture = DiffuseTexture,
				DiffuseTexturePath = DiffuseTexturePath,
			};
		}
	}
}