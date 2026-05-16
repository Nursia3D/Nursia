using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Nursia.Materials;
using Nursia.Rendering;
using System;
using System.Collections.Generic;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// Represents an attachment of a model instance to a bone of a parent model.
	/// Used to attach accessories or child models that follow bone animations.
	/// </summary>
	public class ModelBoneAttachment
	{
		internal NursiaModel NursiaModel { get; } = new NursiaModel();

		/// <summary>
		/// Gets or sets the target bone on the parent model to attach to.
		/// </summary>
		public DrModelBone Bone { get; set; }

		/// <summary>
		/// Gets or sets the local transform applied to the attached model relative to the bone.
		/// Default is identity (no additional transform).
		/// </summary>
		public Matrix Transform { get; set; } = Matrix.Identity;

		private ModelBoneAttachment()
		{
		}

		public static ModelBoneAttachment CreateFromModelNode(NursiaModelNode node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			var result = new ModelBoneAttachment();

			result.NursiaModel.CopyFrom(node.NursiaModel);
			result.Transform = node.LocalTransform;

			return result;
		}
	}

	internal class NursiaModel
	{
		public IMaterial[][] Materials { get; set; }

		public DrModelInstance ModelInstance { get; } = new DrModelInstance();

		public DrModel Model
		{
			get => ModelInstance.Model;

			set => ModelInstance.Model = value;
		}

		public string ModelPath { get; set; }

		public BoundingBox? BoundingBox => ModelInstance.BoundingBox;

		/// <summary>
		/// Gets the collection of models attached to bones of this model instance.
		/// </summary>
		public List<ModelBoneAttachment> BonesAttachments { get; } = new List<ModelBoneAttachment>();

		public NursiaModel()
		{
			ModelInstance.ModelChanged += (s, a) => SetMaterialsFromModel();
		}

		public IMaterial GetMaterial(int meshIndex, int meshPartIndex) => Materials[meshIndex][meshPartIndex];

		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(ModelPath))
			{
				Model = assetManager.LoadModel(Nrs.GraphicsDevice, ModelPath, ModelLoadFlags.EnsureUVs);
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

					Materials[i][j] = BlinnPhongMaterial.FromDrMaterial(meshPart.Material);
				}
			}
		}

		public void AddRenderJobs(Camera camera, IRenderJobsBatch batch, Matrix rootTransform)
		{
			if (ModelInstance == null || Model == null || Materials == null)
			{
				return;
			}

			// Render meshes
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

		public void CopyFrom(NursiaModel model)
		{
			Model = model.Model;
			ModelPath = model.ModelPath;
		}
	}
}
