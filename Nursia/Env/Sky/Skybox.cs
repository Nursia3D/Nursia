using AssetManagementBase;
using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.SceneGraph;
using Nursia.Serialization;
using System;
using System.ComponentModel;

namespace Nursia.Env.Sky
{
	/// <summary>
	/// Represents a skybox for rendering a background environment in a scene.
	/// </summary>
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

			public EffectBinding GetShadowTechnique(MaterialTechnique materialTechnique)
			{
				throw new NotImplementedException();
			}

			public EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool normalMapping, bool clipPlane)
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

		/// <summary>
		/// Gets or sets the diffuse color of the skybox.
		/// </summary>
		public Color DiffuseColor
		{
			get => _material.DiffuseColor;

			set => _material.DiffuseColor = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the skybox is visible.
		/// </summary>
		[DefaultValue(true)]
		public bool Visible { get; set; } = true;

		/// <summary>
		/// Gets or sets the cube texture for the skybox.
		/// </summary>
		[JsonIgnore]
		public TextureCube DiffuseTexture
		{
			get => _material.DiffuseTexture;

			set => _material.DiffuseTexture = value;
		}

		/// <summary>
		/// Gets or sets the path to the cube texture asset.
		/// </summary>
		[Browsable(false)]
		public string DiffuseTexturePath
		{
			get => _material.DiffuseTexturePath;

			set => _material.DiffuseTexturePath = value;
		}

		/// <summary>
		/// Gets or sets the local transformation matrix for the skybox.
		/// </summary>
		[JsonIgnore]
		[Browsable(false)]
		public Matrix LocalTransform { get; set; } = Matrix.CreateScale(100.0f);

		/// <summary>
		/// Initializes a new instance of the <see cref="Skybox"/> class with a default cube mesh.
		/// </summary>
		public Skybox()
		{
			_mesh = MeshPrimitives.CreateBoxMeshPart(Nrs.GraphicsDevice, Vector3.One);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Skybox"/> class with a custom mesh.
		/// </summary>
		/// <param name="mesh">The mesh to use for the skybox.</param>
		public Skybox(DrMeshPart mesh)
		{
			_mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
		}

		/// <summary>
		/// Adds render jobs for this skybox to the specified batch.
		/// </summary>
		/// <param name="camera">The camera used for rendering.</param>
		/// <param name="batch">The render batch to add jobs to.</param>
		public void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			if (!Visible || _material.DiffuseTexture == null)
			{
				return;
			}

			// Calculate special world-view-project matrix with zero translation
			// So the viewer is always in the center
			var view = camera.View;
			view.Translation = Vector3.Zero;

			_material.Transform = LocalTransform * view * camera.Projection;

			batch.AddMesh(_mesh, _material, Matrix.Identity, flags: RenderJobFlags.DontCullByCameraFrustum);
		}

		/// <summary>
		/// Loads external assets referenced by this skybox.
		/// </summary>
		/// <param name="assetManager">The asset manager to load resources from.</param>
		public void Load(AssetManager assetManager)
		{
			_material.Load(assetManager);
		}

		/// <summary>
		/// Creates a copy of this skybox.
		/// </summary>
		/// <returns>A cloned skybox with identical properties.</returns>
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