using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Utilities;
using System;
using System.ComponentModel;

namespace Nursia.SceneGraph.Lights
{
	public enum SpotLightRamp
	{
		Normal,
		Wide
	}

	[EditorInfo("Lights")]
	public class SpotLight : BaseLight
	{
		private bool _dirty = true;
		private Matrix _spotMatrix, _boundingMatrix;
		private float _aspectRatio = 1.0f;
		private float _fovInDegrees = 30.0f;
		private float _range = 10.0f;
		private BoundingFrustum _boundingFrustum;

		[Category("Light")]
		public float AspectRatio
		{
			get => _aspectRatio;

			set
			{
				if (value.EpsilonEquals(_aspectRatio))
				{
					return;
				}

				_aspectRatio = value;
				InvalidateMatrix();
			}
		}

		[Category("Light")]
		public float FieldOfViewInDegrees
		{
			get => _fovInDegrees;

			set
			{
				if (value.EpsilonEquals(_fovInDegrees))
				{
					return;
				}

				_fovInDegrees = value;
				InvalidateMatrix();
			}
		}

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
				InvalidateMatrix();
			}
		}

		[Category("Light")]
		public SpotLightRamp Ramp { get; set; } = SpotLightRamp.Normal;

		[Browsable(false)]
		[JsonIgnore]
		public Matrix SpotMatrix
		{
			get
			{
				UpdateMatrices();

				return _spotMatrix;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public BoundingFrustum Frustum
		{
			get
			{
				UpdateMatrices();

				return _boundingFrustum;
			}
		}

		public SpotLight()
		{
			CastsShadow = false;
		}

		protected override void OnGlobalTransformUpdated()
		{
			base.OnGlobalTransformUpdated();

			InvalidateMatrix();
		}

		private void UpdateMatrices()
		{
			if (!_dirty)
			{
				return;
			}

			var view = Matrix.Invert(GlobalTransform);
			var proj = new Matrix(); // Zero matrix

			// Make the projected light slightly smaller than the shadow map to prevent light spill
			var fovRads = MathHelper.ToRadians(FieldOfViewInDegrees);
			float h = 1.005f / (float)Math.Tan(fovRads * 0.5f);
			float w = h / AspectRatio;
			proj.M11 = w;
			proj.M22 = h;
			proj.M33 = 1.0f / Math.Max(Range, Mathematics.ZeroTolerance);
			proj.M34 = -1.0f;

			var texAdjust = Matrix.Identity;
			texAdjust.Translation = new Vector3(0.5f, 0.5f, 0.0f);
			texAdjust.SetScale(new Vector3(0.5f, -0.5f, 0.5f));

			_spotMatrix = view * proj * texAdjust;

			var boundingProj = Matrix.CreatePerspectiveFieldOfView(fovRads, AspectRatio, 0.01f, Range);
			_boundingMatrix = view * boundingProj;
			if (_boundingFrustum == null)
			{
				_boundingFrustum = new BoundingFrustum(_boundingMatrix);
			}
			else
			{
				_boundingFrustum.Matrix = _boundingMatrix;
			}

			_dirty = false;
		}

		protected override void OnCastsShadowChanged()
		{
		}

		protected override SceneNode CreateInstanceCore() => new SpotLight();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var light = (SpotLight)node;
			AspectRatio = light.AspectRatio;
			FieldOfViewInDegrees = light.FieldOfViewInDegrees;
			Range = light.Range;
			Ramp = light.Ramp;
		}

		private void InvalidateMatrix()
		{
			_dirty = true;
		}

		public override bool AffectsObject(BoundingBox boundingBox)
		{
			UpdateMatrices();

			return _boundingFrustum.Intersects(boundingBox);
		}
	}
}
