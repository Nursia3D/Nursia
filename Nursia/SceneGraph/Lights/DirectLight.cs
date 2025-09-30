using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Shadows;
using System.ComponentModel;

namespace Nursia.SceneGraph.Lights
{
	[EditorInfo("Lights")]
	public partial class DirectLight : BaseLight
	{
		private Vector3 _direction;

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

		protected override SceneNode CreateInstanceCore() => new DirectLight();

		/// <summary>
		/// Directional lights affect all objects
		/// </summary>
		/// <param name="boundingBox"></param>
		/// <returns></returns>
		public override bool AffectsObject(BoundingBox boundingBox) => true;

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
