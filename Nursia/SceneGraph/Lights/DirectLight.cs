using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Shadows;
using System.ComponentModel;

namespace Nursia.SceneGraph.Lights
{
	/// <summary>
	/// Represents a directional light that illuminates the entire scene.
	/// </summary>
	[EditorInfo("Lights")]
	public partial class DirectLight : BaseLight
	{
		private Vector3 _direction;

		/// <summary>
		/// Gets or sets the direction in which this light shines.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Vector3 Direction
		{
			get
			{
				UpdateGlobalTransform();
				return _direction;
			}

			set
			{
				LocalTransform = Matrix.Invert(Matrix.CreateLookAt(Vector3.Zero, value, Vector3.Up));
				UpdateGlobalTransform();

				_direction = value;
			}
		}

		internal float InternalShadowBase => Nrs.GraphicsSettings.ShadowBase;

		internal float InternalShadowIntensity => Nrs.GraphicsSettings.ShadowIntensity;

		internal float InternalShadowBias => Nrs.GraphicsSettings.ShadowBias;

		internal float InternalShadowFadeStart => Nrs.GraphicsSettings.ShadowFadeStart;

		internal ShadowCascadeManager ShadowCascadeManager => Nrs.GraphicsSettings.ShadowCascadeManager;

		/// <summary>
		/// Creates a copy of this direct light node.
		/// </summary>
		/// <returns>A new DirectLight instance.</returns>
		protected override SceneNode CreateInstanceCore() => new DirectLight();

		/// <summary>
		/// Determines if this directional light affects an object.
		/// </summary>
		/// <remarks>
		/// Directional lights affect all objects in the scene.
		/// </remarks>
		/// <param name="boundingBox">The bounding box to test.</param>
		/// <returns>Always returns <c>true</c> for directional lights.</returns>
		public override bool AffectsObject(BoundingBox boundingBox) => true;

		/// <summary>
		/// Called when the global transform is updated.
		/// </summary>
		protected override void OnGlobalTransformUpdated()
		{
			base.OnGlobalTransformUpdated();

			var tr = GlobalTransform;

			var result = tr.Forward;
			result.Normalize();
			_direction = result;
		}
	}
}
