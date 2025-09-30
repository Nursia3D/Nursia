using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nursia;
using Nursia.SceneGraph;

namespace FuelCell
{
	public class FuelCell : GameObject
	{
		public bool Retrieved { get; set; }
		private SoundEffect fuelCellCollect;

		public FuelCell()
			: base()
		{
			Retrieved = false;
		}

		public void LoadContent(AssetManager content, string modelName)
		{
			Model = (NursiaModelNode)content.LoadSceneNode(modelName);
			BoundingSphere = CalculateBoundingSphere();
			Position = Vector3.Down;

			fuelCellCollect = content.LoadSoundEffect("Audio/fuelcell-collect.wav");

			BoundingSphere scaledSphere;
			scaledSphere = BoundingSphere;
			scaledSphere.Radius *= GameConstants.FuelCellBoundingSphereFactor;
			BoundingSphere = new BoundingSphere(scaledSphere.Center, scaledSphere.Radius);
		}

		internal void Update(BoundingSphere vehicleBoundingSphere)
		{
			if (vehicleBoundingSphere.Intersects(this.BoundingSphere) && !this.Retrieved)
			{
				this.Retrieved = true;
				fuelCellCollect.Play();
			}
		}
	}
}