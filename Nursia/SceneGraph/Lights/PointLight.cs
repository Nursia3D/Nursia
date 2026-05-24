using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Lights
{
	/// <summary>
	/// Specifies the falloff curve for point light intensity.
	/// </summary>
	public enum PointLightRamp
	{
		/// <summary>Normal falloff curve.</summary>
		Normal,

		/// <summary>Wide falloff curve.</summary>
		Wide,

		/// <summary>Extreme falloff curve.</summary>
		Extreme
	}

	/// <summary>
	/// Represents a point light that radiates light in all directions from a location.
	/// </summary>
	[EditorInfo("Lights")]
	public class PointLight : BaseLight
	{
		private BoundingSphere? _boundingSphere = null;

		/// <summary>
		/// The effective range of this point light.
		/// </summary>
		public float _range = 10.0f;

		/// <summary>
		/// Gets or sets the effective range of this point light.
		/// </summary>
		[Category("Light")]
		public float Range
		{
			get => _range;

			set
			{
				if (value.EpsilonEquals(_range))
				{
					return;
				}

				_range = value;
				_boundingSphere = null;
			}
		}

		/// <summary>
		/// Gets or sets the falloff curve for light intensity.
		/// </summary>
		[Category("Light")]
		public PointLightRamp Ramp { get; set; } = PointLightRamp.Normal;

		private BoundingSphere BoundingSphere
		{
			get
			{
				if (_boundingSphere != null)
				{
					return _boundingSphere.Value;
				}

				_boundingSphere = new BoundingSphere(GlobalTransform.Translation, Range);
				return _boundingSphere.Value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PointLight"/> class.
		/// </summary>
		public PointLight()
		{
			CastsShadow = false;
		}

		/// <summary>
		/// Called when the CastsShadow property changes.
		/// </summary>
		protected override void OnCastsShadowChanged()
		{
		}

		/// <summary>
		/// Creates a copy of this point light node.
		/// </summary>
		/// <returns>A new PointLight instance.</returns>
		protected override SceneNode CreateInstanceCore() => new PointLight();

		/// <summary>
		/// Copies light properties from another node.
		/// </summary>
		/// <param name="node">The source node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var light = (PointLight)node;
			Range = light.Range;
			Ramp = light.Ramp;
		}

		/// <summary>
		/// Called when the global transform is updated.
		/// </summary>
		protected override void OnGlobalTransformUpdated()
		{
			base.OnGlobalTransformUpdated();

			_boundingSphere = null;
		}

		/// <summary>
		/// Determines if this point light affects the specified object.
		/// </summary>
		/// <param name="boundingBox">The bounding box to test.</param>
		/// <returns><c>true</c> if the light's range intersects the bounding box; otherwise, <c>false</c>.</returns>
		public override bool AffectsObject(BoundingBox boundingBox)
		{
			return BoundingSphere.Intersects(boundingBox);
		}
	}
}
