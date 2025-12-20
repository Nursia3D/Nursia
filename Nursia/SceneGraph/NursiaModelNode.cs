using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.Serialization;
using System;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	[EditorInfo("Base")]
	public class NursiaModelNode : SceneNode
	{
		/// <summary>
		/// TODO: More sophisticated algorithm of invalidating the render jobs
		/// </summary>
		public override SceneFlags Flags => SceneFlags.HasRenderJobs;

		[Browsable(false)]
		public IMaterial[][] Materials { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public DrModelInstance ModelInstance { get; } = new DrModelInstance();

		[JsonIgnore]
		[Category("Resources")]
		public DrModel Model
		{
			get => ModelInstance.Model;

			set => SetModel(value, true);
		}

		[Browsable(false)]
		public string ModelPath { get; set; }

		public override BoundingBox? BoundingBox => ModelInstance.BoundingBox;

		public override void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			base.AddRenderJobs(camera, batch);

			if (Model == null || Materials == null)
			{
				return;
			}

			// Render meshes
			var rootTransform = GlobalTransform;
			for (var i = 0; i < Math.Min(Model.Meshes.Length, Materials.Length); ++i)
			{
				var mesh = Model.Meshes[i];

				var bone = mesh.ParentBone;
				for (var j = 0; j < Math.Min(mesh.MeshParts.Count, Materials[i].Length); ++j)
				{
					var material = Materials[i][j];
					if (material == null)
					{
						continue;
					}

					var part = mesh.MeshParts[j];

					// If mesh has bones, then parent node transform had been already
					// applied to bones transform
					// Thus to avoid applying parent transform twice, we use
					// ordinary Transform(not AbsoluteTransform) for parts with bones
					Matrix transform = part.Skin != null ? rootTransform : ModelInstance.GetBoneGlobalTransform(bone.Index) * rootTransform;

					// Apply the effect and render items
					Matrix[] bonesTransforms = null;
					if (part.Skin != null)
					{
						bonesTransforms = ModelInstance.GetSkinTransforms(part.Skin.SkinIndex);
					}

					batch.AddMesh(part, material, transform, bonesTransforms: bonesTransforms);
				}
			}
		}

		public IMaterial GetMaterial(int meshIndex, int meshPartIndex) => Materials[meshIndex][meshPartIndex];

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (!string.IsNullOrEmpty(ModelPath))
			{
				var model = assetManager.LoadModel(Nrs.GraphicsDevice, ModelPath, ModelLoadFlags.EnsureUVs);
				SetModel(model, false);
			}

			if (Materials != null)
			{
				for (var i = 0; i < Materials.Length; ++i)
				{
					for (var j = 0; j < Materials[i].Length; ++j)
					{
						var external = Materials[i][j] as IHasExternalAssets;
						if (external != null)
						{
							external.Load(assetManager);
						}
					}
				}
			}
		}

		public void SetMaterialsFromModel()
		{
			if (Model == null)
			{
				Materials = null;
				return;
			}

			Materials = new IMaterial[Model.Meshes.Length][];

			for (var i = 0; i < Model.Meshes.Length; ++i)
			{
				var mesh = Model.Meshes[i];

				Materials[i] = new IMaterial[mesh.MeshParts.Count];
				for (var j = 0; j < mesh.MeshParts.Count; ++j)
				{
					var meshPart = mesh.MeshParts[j];
					if (meshPart.Material == null)
					{
						continue;
					}

					Materials[i][j] = new BlinnPhongMaterial
					{
						DiffuseColor = meshPart.Material.DiffuseColor,
						DiffuseTexture = meshPart.Material.DiffuseTexture,
						NormalTexture = meshPart.Material.NormalTexture
					};
				}
			}
		}

		public void SetModel(DrModel model, bool setMaterials)
		{
			ModelInstance.Model = model;

			if (setMaterials)
			{
				SetMaterialsFromModel();
			}
		}

		protected override SceneNode CreateInstanceCore() => new NursiaModelNode();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var model = (NursiaModelNode)node;
			if (model.Materials != null)
			{
				Materials = new IMaterial[model.Materials.Length][];
				for (var i = 0; i < model.Materials.Length; ++i)
				{
					Materials[i] = new IMaterial[model.Materials[i].Length];

					for (var j = 0; j < model.Materials[i].Length; ++j)
					{
						Materials[i][j] = model.Materials[i][j]?.Clone();
					}
				}
			}

			SetModel(model.Model, false);
			ModelPath = model.ModelPath;
		}
	}
}
