using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.SceneGraph;

namespace FuelCell
{
	public class GameObject
	{
		public NursiaModelNode Model { get; set; }
		public Vector3 Position
		{
			get => Model.Translation;

			set => Model.Translation = value;
		}
		
		public bool IsActive { get; set; }
		public BoundingSphere BoundingSphere { get; set; }

		public GameObject()
		{
			Model = null;
			IsActive = false;
			BoundingSphere = new BoundingSphere();
		}

		protected BoundingSphere CalculateBoundingSphere() => BoundingSphere.CreateFromBoundingBox(Model.BoundingBox.Value);
	}
}