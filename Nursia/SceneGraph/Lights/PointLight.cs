using Microsoft.Xna.Framework;
using Nursia.Attributes;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph.Lights
{
	public enum PointLightRamp
	{
		Normal,
		Wide,
		Extreme
	}

	[EditorInfo("Lights")]
	public class PointLight : BaseLight
	{
		private BoundingSphere? _boundingSphere = null;
		public float _range = 10.0f;

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

		public PointLight()
		{
			CastsShadow = false;
		}

		protected override void OnCastsShadowChanged()
		{
		}

		protected override SceneNode CreateInstanceCore() => new PointLight();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var light = (PointLight)node;
			Range = light.Range;
			Ramp = light.Ramp;
		}

		protected override void OnGlobalTransformUpdated()
		{
			base.OnGlobalTransformUpdated();

			_boundingSphere = null;
		}

		public override bool AffectsObject(BoundingBox boundingBox)
		{
			return BoundingSphere.Intersects(boundingBox);
		}
	}
}
