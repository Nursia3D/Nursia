using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// A scene node that renders a single mesh-material combination multiple times using hardware instancing.
	/// </summary>
	public class InstancedMeshNode : SceneNode
	{
		// To store instance transform matrices in a vertex buffer, we use this custom
		// vertex type which encodes 4x4 matrices as a set of four Vector4 values.
		private static VertexDeclaration _instanceVertexDeclaration = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
			new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
			new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
			new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
		);

		private DynamicVertexBuffer _vertexBuffer;
		private BoundingBox _boundingBox;

		/// <summary>
		/// Gets the flags for this node.
		/// </summary>
		public override SceneFlags Flags => SceneFlags.HasRenderJobs;

		/// <summary>
		/// Gets or sets the mesh to render.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public DrMeshPart Mesh { get; set; }

		/// <summary>
		/// Gets or sets the material for the mesh.
		/// </summary>
		[DefaultMaterial]
		public IMaterial Material { get; set; }

		/// <summary>
		/// Gets the bounding box of this node.
		/// </summary>
		public override BoundingBox? BoundingBox => _boundingBox;

		/// <summary>
		/// Gets the collection of transformation matrices for the instances.
		/// </summary>
		public ObservableCollection<Matrix> InstancesTransforms { get; } = new ObservableCollection<Matrix>();

		/// <summary>
		/// Initializes a new instance of the <see cref="InstancedMeshNode"/> class.
		/// </summary>
		public InstancedMeshNode()
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

		/// <summary>
		/// Adds render jobs for all instances to the specified batch.
		/// </summary>
		/// <param name="camera">The camera used for rendering.</param>
		/// <param name="batch">The render batch to add jobs to.</param>
		public override void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
			base.AddRenderJobs(camera, batch);

			if (Mesh == null || Material == null || InstancesTransforms.Count == 0)
			{
				return;
			}

			if (_vertexBuffer == null)
			{
				// Update buffer and bounding box
				var transforms = new Matrix[InstancesTransforms.Count];
				_vertexBuffer = new DynamicVertexBuffer(Nrs.GraphicsDevice, _instanceVertexDeclaration, transforms.Length, BufferUsage.WriteOnly);

				for (var i = 0; i < transforms.Length; ++i)
				{
					var transform = InstancesTransforms[i];

					// We need to transpose transform for them to properly passed in the shader
					transforms[i] = Matrix.Transpose(transform);

					if (i == 0)
					{
						_boundingBox = Mesh.BoundingBox.Transform(ref transform);
					}
					else
					{
						_boundingBox = Microsoft.Xna.Framework.BoundingBox.CreateMerged(_boundingBox, Mesh.BoundingBox.Transform(ref transform));
					}
				}

				_vertexBuffer.SetData(transforms, 0, transforms.Length, SetDataOptions.Discard);
			}

			batch.AddInstancedMesh(Mesh, Material, GlobalTransform, _vertexBuffer, _boundingBox);
		}
	}
}
