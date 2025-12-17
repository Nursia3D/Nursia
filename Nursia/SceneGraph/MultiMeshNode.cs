using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		// To store instance transform matrices in a vertex buffer, we use this custom
		// vertex type which encodes 4x4 matrices as a set of four Vector4 values.
		static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
			new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
			new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
			new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3)
		);

		private DynamicVertexBuffer _vertexBuffer;

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
			if (_vertexBuffer != null)
			{
				_vertexBuffer.Dispose();
				_vertexBuffer = null;
			}
		}

		protected internal override void Render(IRenderBatch batch)
		{
			base.Render(batch);

			if (Mesh == null || Material == null || InstancesTransforms.Count == 0)
			{
				return;
			}

			if (_vertexBuffer == null)
			{
				// Update transforms
				var transforms = new Matrix[InstancesTransforms.Count];
				_vertexBuffer = new DynamicVertexBuffer(Nrs.GraphicsDevice, instanceVertexDeclaration, transforms.Length, BufferUsage.WriteOnly);

				for (var i = 0; i < transforms.Length; ++i)
				{
					transforms[i] = InstancesTransforms[i];
				}

				_vertexBuffer.SetData(transforms, 0, transforms.Length, SetDataOptions.Discard);
			}

			batch.BatchJob(Material, GlobalTransform, Mesh, instancesTransforms: _vertexBuffer);
		}
	}
}
