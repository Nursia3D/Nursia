using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Utilities;
using System;
using System.ComponentModel;

namespace Nursia.SceneGraph.Lights
{
	/// <summary>
	/// Specifies the falloff curve for spot light intensity.
	/// </summary>
	public enum SpotLightRamp
	{
		/// <summary>Normal falloff curve.</summary>
		Normal,

		/// <summary>Wide falloff curve.</summary>
		Wide
	}

	/// <summary>
	/// Represents a spot light that creates a cone of light in a specific direction.
	/// </summary>
	[EditorInfo("Lights")]
	public class SpotLight : BaseLight
	{
		private bool _dirty = true;
		private Matrix _spotMatrix, _boundingMatrix;
		private float _aspectRatio = 1.0f;
		private float _fovInDegrees = 30.0f;
		private float _range = 10.0f;
		private BoundingFrustum _boundingFrustum;

		/// <summary>
		/// Gets or sets the aspect ratio of the light cone.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the field of view angle of the light cone in degrees.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the effective range of this spot light.
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
				InvalidateMatrix();
			}
		}

		/// <summary>
		/// Gets or sets the falloff curve for light intensity.
		/// </summary>
		[Category("Light")]
		public SpotLightRamp Ramp { get; set; } = SpotLightRamp.Normal;

		/// <summary>
		/// Gets the transformation matrix used for spot light projection.
		/// </summary>
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

		/// <summary>
		/// Gets the bounding frustum of this spot light.
		/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="SpotLight"/> class.
		/// </summary>
		public SpotLight()
		{
			CastsShadow = false;
		}

		/// <summary>
		/// Called when the global transform is updated.
		/// </summary>
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

		/// <summary>
		/// Called when the CastsShadow property changes.
		/// </summary>
		protected override void OnCastsShadowChanged()
		{
		}

		/// <summary>
		/// Creates a copy of this spot light node.
		/// </summary>
		/// <returns>A new SpotLight instance.</returns>
		protected override SceneNode CreateInstanceCore() => new SpotLight();

		/// <summary>
		/// Copies light properties from another node.
		/// </summary>
		/// <param name="node">The source node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var light = (SpotLight)node;
			AspectRatio = light.AspectRatio;
			FieldOfViewInDegrees = light.FieldOfViewInDegrees;
			Range = light.Range;
			Ramp = light.Ramp;
		}

		/// <summary>
		/// Marks the transformation matrices as dirty and requiring recalculation.
		/// </summary>
		private void InvalidateMatrix()
		{
			_dirty = true;
		}

		/// <summary>
		/// Determines if this spot light affects the specified object.
		/// </summary>
		/// <param name="boundingBox">The bounding box to test.</param>
		/// <returns><c>true</c> if the light cone intersects the bounding box; otherwise, <c>false</c>.</returns>
		public override bool AffectsObject(BoundingBox boundingBox)
		{
			UpdateMatrices();

			return _boundingFrustum.Intersects(boundingBox);
		}
	}
}
