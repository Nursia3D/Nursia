using AssetManagementBase;
using Microsoft.Xna.Framework;
using Nursia;
using Nursia.SceneGraph;

namespace FuelCell
{
	public class Barrier : GameObject
	{
		public string BarrierType { get; set; }

		public Barrier()
			: base()
		{
			BarrierType = null;
		}

		public void LoadContent(AssetManager content, string modelName)
		{
			Model = (NursiaModelNode)content.LoadSceneNode(modelName);
			BarrierType = modelName;
			BoundingSphere = CalculateBoundingSphere();
			Position = Vector3.Down;

			BoundingSphere scaledSphere;
			scaledSphere = BoundingSphere;
			scaledSphere.Radius *= GameConstants.BarrierBoundingSphereFactor;
			BoundingSphere = new BoundingSphere(scaledSphere.Center, scaledSphere.Radius);
		}
	}
}