using DigitalRiseModel;
using DigitalRiseModel.Primitives;
using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Rendering;
using Nursia.Samples.ThirdPerson.Materials;
using Nursia.SceneGraph;

namespace Nursia.Samples.ThirdPerson
{
	[EditorInfo("Base")]
	public class ReflectionPlane : SceneNode
	{
		private readonly PlaneReflectMaterial _material;

		private readonly DrMeshPart _planeMesh;

		public override BoundingBox? BoundingBox => _planeMesh.BoundingBox;

		public Color DiffuseColor
		{
			get => _material.DiffuseColor;
			set => _material.DiffuseColor = value;
		}

		public ReflectionPlane()
		{
			_material = new PlaneReflectMaterial();
			_planeMesh = MeshPrimitives.CreatePlaneMeshPart(Nrs.GraphicsDevice, normalDirection: NormalDirection.UpZ);
		}

		protected override void Render(IRenderBatch batch)
		{
			base.Render(batch);

			batch.BatchJob(_material, GlobalTransform, _planeMesh, reflectionPlane: new Plane(Vector3.UnitZ, 0));
		}

		protected override SceneNode CreateInstanceCore() => new ReflectionPlane();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var reflectionPlane = (ReflectionPlane)node;
			DiffuseColor = reflectionPlane.DiffuseColor;
		}
	}
}
