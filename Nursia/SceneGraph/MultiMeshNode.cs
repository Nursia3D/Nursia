using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Materials;
using Nursia.Rendering;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// Does the hardware instancing
	/// </summary>
	public class MultiMeshNode : SceneNode
	{
		private Matrix[] _transforms;
		private bool _transformsDirty = true;

		[Browsable(false)]
		[JsonIgnore]
		public DrMeshPart Mesh { get; set; }
		[DefaultMaterial]
		public IMaterial Material { get; set; }

		public ObservableCollection<Matrix> InstancesTransforms { get; } = new ObservableCollection<Matrix>();

		public MultiMeshNode()
		{
			InstancesTransforms.CollectionChanged += InstancesTransforms_CollectionChanged;
		}

		private void InstancesTransforms_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_transforms = null;
			_transformsDirty = true;
		}

		public override void InvalidateTransform()
		{
			base.InvalidateTransform();
			_transformsDirty = true;
		}

		protected internal override void Render(IRenderBatch batch)
		{
			base.Render(batch);

			if (Mesh == null || Material == null || InstancesTransforms.Count == 0)
			{
				return;
			}

			// Update transforms
			if (_transforms == null)
			{
				_transforms = new Matrix[InstancesTransforms.Count];
			}

			if (_transformsDirty)
			{
				for(var i = 0; i < _transforms.Length; ++i)
				{
					_transforms[i] = InstancesTransforms[i] * GlobalTransform;
				}

				_transformsDirty = false;
			}

			batch.BatchJob(Material, _transforms[0], Mesh, instancesTransforms: _transforms);
		}
	}
}
