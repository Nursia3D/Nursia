using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Utilities;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// Specifies the type of camera projection.
	/// </summary>
	public enum CameraType
	{
		/// <summary>Perspective projection (3D view).</summary>
		Perspective,

		/// <summary>Orthographic projection (2D view).</summary>
		Orthographic
	}

	/// <summary>
	/// Represents a camera in a Nursia 3D scene.
	/// </summary>
	public class Camera : SceneNode
	{
		private CameraType _cameraType = CameraType.Perspective;
		private float _viewAngle = 90.0f;
		private float _width = 1200.0f, _height = 800.0f;
		private float _nearPlane = 0.1f;
		private float _farPlane = 1000f;
		private Matrix? _reflectionMatrix;
		private Matrix? _view, _customView;
		private Matrix _projection = Matrix.Identity;
		private Matrix? _inverseView;
		private BoundingFrustum _frustum;
		private bool _dirty = true;

		/// <summary>
		/// Gets or sets the type of camera projection.
		/// </summary>
		[Category("Camera")]
		public CameraType CameraType
		{
			get => _cameraType;

			set
			{
				if (value == _cameraType)
				{
					return;
				}

				_cameraType = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the field of view angle in degrees for perspective cameras.
		/// </summary>
		[Category("Camera")]
		public float ViewAngle
		{
			get => _viewAngle;

			set
			{
				if (value.EpsilonEquals(_viewAngle))
				{
					return;
				}

				_viewAngle = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the distance to the near clipping plane.
		/// </summary>
		[Category("Camera")]
		public float NearPlane
		{
			get => _nearPlane;

			set
			{
				if (value.EpsilonEquals(_nearPlane))
				{
					return;
				}

				_nearPlane = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the distance to the far clipping plane.
		/// </summary>
		[Category("Camera")]
		public float FarPlane
		{
			get => _farPlane;

			set
			{
				if (value.EpsilonEquals(_farPlane))
				{
					return;
				}

				_farPlane = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the optional reflection matrix for water/mirror effects.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix? ReflectionMatrix
		{
			get => _reflectionMatrix;

			set
			{
				_reflectionMatrix = value;
				Invalidate();
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		internal float Width
		{
			get => _width;

			set
			{
				if (value.EpsilonEquals(_width))
				{
					return;
				}

				_width = value;
				Invalidate();
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		internal float Height
		{
			get => _height;

			set
			{
				if (value.EpsilonEquals(_height))
				{
					return;
				}

				_height = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the view matrix that transforms world coordinates to camera space.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix View
		{
			get
			{
				if (_customView != null)
				{
					return _customView.Value;
				}

				if (_view == null)
				{
					// Calculate from GlobalTransform
					_view = Matrix.Invert(GlobalTransform);
					_inverseView = null;
				}

				return _view.Value;
			}

			set
			{
				// Update GlobalTransform from explicit View matrix
				GlobalTransform = Matrix.Invert(value);

				// Set the value
				_customView = value;
				_inverseView = null;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets the inverse view matrix that transforms camera space to world coordinates.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix InverseView
		{
			get
			{
				if (_inverseView == null)
				{
					_inverseView = Matrix.Invert(View);
				}

				return _inverseView.Value;
			}
		}

		/// <summary>
		/// Gets the projection matrix that transforms camera space to normalized device coordinates.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix Projection
		{
			get
			{
				Update();

				return _projection;
			}
		}

		/// <summary>
		/// Gets the combined view-projection matrix for rendering.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix ViewProjection
		{
			get
			{
				Update();

				return _frustum.Matrix;
			}
		}

		/// <summary>
		/// Gets the camera's view frustum for visibility culling.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public BoundingFrustum Frustum
		{
			get
			{
				Update();

				return _frustum;
			}
		}

		/// <summary>
		/// Invalidates the camera's transform and its derived matrices.
		/// </summary>
		public override void InvalidateTransform()
		{
			base.InvalidateTransform();

			_customView = null;
			_view = null;
			_inverseView = null;
			Invalidate();
		}

		/// <summary>
		/// Marks the projection matrix as dirty and requiring recalculation.
		/// </summary>
		protected void Invalidate()
		{
			_dirty = true;
		}

		/// <summary>
		/// Calculates a projection matrix for the specified near and far planes.
		/// </summary>
		/// <param name="near">The distance to the near clipping plane.</param>
		/// <param name="far">The distance to the far clipping plane.</param>
		/// <returns>The calculated projection matrix.</returns>
		public Matrix CalculateProjection(float near, float far)
		{
			if (_cameraType == CameraType.Perspective)
			{
				var aspectRatio = Width / Height;
				return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(_viewAngle), aspectRatio, near, far);
			}

			return Matrix.CreateOrthographic(Width, Height, near, far);
		}

		private void Update()
		{
			if (!_dirty)
			{
				return;
			}

			_projection = CalculateProjection(NearPlane, FarPlane);
			var viewProjection = View * _projection;

			if (ReflectionMatrix != null)
			{
				viewProjection = ReflectionMatrix.Value * viewProjection;
			}

			if (_frustum == null)
			{
				_frustum = new BoundingFrustum(viewProjection);
			}
			else
			{
				_frustum.Matrix = viewProjection;
			}

			_dirty = false;
		}

		/// <summary>
		/// Creates a new instance of the Camera class.
		/// </summary>
		protected override SceneNode CreateInstanceCore() => new Camera();

		/// <summary>
		/// Copies all view parameters from another camera.
		/// </summary>
		/// <param name="source">The source camera to copy parameters from.</param>
		public void CopyViewParams(Camera source)
		{
			View = source.View;
			CameraType = source.CameraType;
			ViewAngle = source.ViewAngle;
			Width = source.Width;
			Height = source.Height;
			NearPlane = source.NearPlane;
			FarPlane = source.FarPlane;
			ReflectionMatrix = source.ReflectionMatrix;
		}

		/// <summary>
		/// Copies all camera properties from another camera.
		/// </summary>
		/// <param name="node">The source camera to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var camera = (Camera)node;
			CameraType = camera.CameraType;
			ViewAngle = camera.ViewAngle;
			Width = camera.Width;
			Height = camera.Height;
			NearPlane = camera.NearPlane;
			FarPlane = camera.FarPlane;
			ReflectionMatrix = camera.ReflectionMatrix;
			_customView = camera._customView;
		}
	}
}