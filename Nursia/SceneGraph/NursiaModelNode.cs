using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Materials;
using Nursia.Rendering;
using System.Collections.Generic;
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

		internal NursiaModel NursiaModel { get; } = new NursiaModel
		{
			ModelInstance = new DrModelInstance()
		};

		[Browsable(false)]
		public IMaterial[][] Materials
		{
			get => NursiaModel.Materials;

			set => NursiaModel.Materials = value;
		}

		[Browsable(false)]
		[JsonIgnore]
		public DrModelInstance ModelInstance => NursiaModel.ModelInstance;

		[JsonIgnore]
		[Category("Resources")]
		public DrModel Model
		{
			get => NursiaModel.Model;

			set => NursiaModel.Model = value;
		}

		[Browsable(false)]
		public string ModelPath
		{
			get => NursiaModel.ModelPath;

			set => NursiaModel.ModelPath = value;
		}

		public override BoundingBox? BoundingBox => NursiaModel.BoundingBox;

		/// <summary>
		/// Gets the collection of models attached to bones of this model instance.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public List<ModelBoneAttachment> BonesAttachments { get; } = new List<ModelBoneAttachment>();

		public override void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			base.AddRenderJobs(camera, batch);

			if (Model == null || Materials == null)
			{
				return;
			}

			NursiaModel.AddRenderJobs(camera, batch, GlobalTransform);

			foreach (var attachment in BonesAttachments)
			{
				if (attachment.ModelInstance == null || attachment.Bone == null)
				{
					continue;
				}

				var transform = attachment.Transform * ModelInstance.GetBoneGlobalTransform(attachment.Bone.Index) * GlobalTransform;
				attachment.InternalModel.AddRenderJobs(camera, batch, transform);
			}
		}

		public IMaterial GetMaterial(int meshIndex, int meshPartIndex) => Materials[meshIndex][meshPartIndex];

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			NursiaModel.Load(assetManager);
		}

		public void SetMaterialsFromModel() => NursiaModel.SetMaterialsFromModel();

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

			NursiaModel.SetModel(model.Model, false);
			ModelPath = model.ModelPath;
		}
	}
}
