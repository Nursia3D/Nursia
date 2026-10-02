using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nursia.SceneGraph;

namespace Nursia.Utilities
{
	/// <summary>
	/// Handles keyboard and mouse input for controlling a camera in real-time.
	/// </summary>
	/// <remarks>
	/// The controller owns the camera orientation as a yaw and pitch relative to
	/// <see cref="Up"/> and rebuilds <see cref="Camera.View"/> from it, so the camera can be
	/// used in a world whose up axis is not <see cref="Vector3.Up"/> (for example Z up).
	/// </remarks>
	public class CameraInputController
	{
		private const float MaxPitch = 89.9f;

		private Point _lastMousePosition;
		private Vector3 _up = Vector3.Up;
		private Vector3 _eye;
		private float _yaw;
		private float _pitch;

		/// <summary>
		/// Gets the camera being controlled.
		/// </summary>
		public Camera Camera { get; }

		/// <summary>
		/// Gets or sets the camera position in world space.
		/// </summary>
		/// <remarks>
		/// This is authoritative for movement and for building the view matrix. It is tracked
		/// separately from <see cref="SceneNode.Translation"/> because assigning
		/// <see cref="Camera.View"/> rewrites the camera transform through an Euler round trip
		/// that loses precision, which would otherwise accumulate every time the view is rebuilt.
		/// </remarks>
		public Vector3 Eye
		{
			get => _eye;

			set
			{
				_eye = value;
				WriteView();
			}
		}

		/// <summary>
		/// Moves the camera by a world space offset and rebuilds the view matrix.
		/// </summary>
		/// <param name="offset">The world space offset to move by.</param>
		public void Translate(Vector3 offset)
		{
			if (offset == Vector3.Zero)
				return;

			_eye += offset;
			WriteView();
		}

		/// <summary>
		/// Gets or sets the world up axis. Never zero length.
		/// </summary>
		/// <remarks>
		/// Changing this reinterprets the current yaw and pitch, so the view is rebuilt.
		/// </remarks>
		public Vector3 Up
		{
			get => _up;

			set
			{
				var lengthSquared = value.LengthSquared();
				_up = lengthSquared <= 0f ? Vector3.Up : value / (float)Math.Sqrt(lengthSquared);

				ApplyRotation();
			}
		}

		/// <summary>
		/// Gets or sets the camera yaw in degrees, rotating about <see cref="Up"/>.
		/// </summary>
		public float Yaw
		{
			get => _yaw;

			set
			{
				_yaw = value;
				ApplyRotation();
			}
		}

		/// <summary>
		/// Gets or sets the camera pitch in degrees, positive looking up, clamped just short of straight up.
		/// </summary>
		public float Pitch
		{
			get => _pitch;

			set
			{
				_pitch = MathHelper.Clamp(value, -MaxPitch, MaxPitch);
				ApplyRotation();
			}
		}

		/// <summary>
		/// Gets the current world space direction the camera looks along.
		/// </summary>
		public Vector3 Forward => CreateForward(_yaw, _pitch, _up);

		/// <summary>
		/// Gets or sets the camera movement speed.
		/// </summary>
		public float MoveSpeed { get; set; } = 10.0f;

		/// <summary>
		/// Gets or sets the camera rotation speed.
		/// </summary>
		public float RotationSpeed { get; set; } = 0.1f;

		/// <summary>
		/// Gets or sets the movement speed multiplier when sprinting.
		/// </summary>
		public float SprintMultiplier { get; set; } = 2.0f;

		/// <summary>
		/// Gets or sets the movement speed multiplier applied after the focus distance scaling.
		/// </summary>
		public float MoveSpeedFactor { get; set; } = 1.0f;

		/// <summary>
		/// Initializes a new instance of the <see cref="CameraInputController"/> class.
		/// </summary>
		/// <param name="camera">The camera to control.</param>
		public CameraInputController(Camera camera)
		{
			Camera = camera;
			_eye = camera.Translation;

			var mouse = Mouse.GetState();
			_lastMousePosition = new Point(mouse.X, mouse.Y);
		}

		/// <summary>
		/// Updates the camera based on current keyboard and mouse input.
		/// </summary>
		public void Update() => Update(0.016f);

		/// <summary>
		/// Updates the camera based on current keyboard and mouse input.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time since the last update in seconds.</param>
		public void Update(float elapsedSeconds)
		{
			UpdateMovement(elapsedSeconds);
			UpdateRotation();
		}

		/// <summary>
		/// Points the camera along a world direction, deriving yaw and pitch from it.
		/// Does nothing when the direction is parallel to <see cref="Up"/>.
		/// </summary>
		/// <param name="forward">The world space direction to look along.</param>
		public void LookAlong(Vector3 forward)
		{
			var up = _up;

			var along = Vector3.Dot(forward, up);
			var horizontal = forward - up * along;

			var lengthSquared = horizontal.LengthSquared();
			if (lengthSquared <= 0f)
			{
				// Looking straight up or down: keep the current yaw.
				_pitch = MathHelper.ToDegrees((float)Math.Asin(MathHelper.Clamp(along, -1f, 1f)));
				ApplyRotation();
				return;
			}

			horizontal /= (float)Math.Sqrt(lengthSquared);

			var reference = GetReferenceForward(up);
			_pitch = MathHelper.ToDegrees((float)Math.Asin(MathHelper.Clamp(along, -1f, 1f)));
			_yaw = MathHelper.ToDegrees((float)Math.Atan2(Vector3.Dot(Vector3.Cross(reference, horizontal), up), Vector3.Dot(reference, horizontal)));

			ApplyRotation();
		}

		/// <summary>
		/// Rebuilds <see cref="Camera.View"/> from the current eye, yaw, pitch and <see cref="Up"/>,
		/// adopting a camera that was moved by something else.
		/// </summary>
		/// <remarks>
		/// Use <see cref="Eye"/> or <see cref="Translate"/> to move the camera, since those are
		/// authoritative and will not be undone here.
		/// </remarks>
		public void ApplyRotation()
		{
			// The round trip through Camera.View leaves rounding residue in the camera transform,
			// proportional to the distance from the origin. Anything beyond that residue means the
			// camera was moved by something else, which this controller has to pick up.
			var cameraTranslation = Camera.Translation;
			var snapThreshold = Math.Max(1e-3, _eye.Length() * 1e-4);
			if (Vector3.Distance(cameraTranslation, _eye) > snapThreshold)
			{
				_eye = cameraTranslation;
			}

			WriteView();
		}

		private void WriteView()
		{
			// Re-seed the translation from the exact eye every time so that the rounding residue
			// cannot accumulate frame over frame.
			if (Camera.Translation != _eye)
			{
				Camera.Translation = _eye;
			}

			Camera.View = CreateViewMatrix(_eye, _yaw, _pitch, _up);
		}

		/// <summary>
		/// Creates a world to camera view matrix for the given eye position and orientation.
		/// </summary>
		/// <param name="eye">The camera position in world space.</param>
		/// <param name="yawDegrees">The yaw in degrees, rotating about <paramref name="up"/>.</param>
		/// <param name="pitchDegrees">The pitch in degrees, positive looking up.</param>
		/// <param name="up">The world up axis.</param>
		/// <returns>The view matrix.</returns>
		public static Matrix CreateViewMatrix(Vector3 eye, float yawDegrees, float pitchDegrees, Vector3 up)
		{
			var forward = CreateForward(yawDegrees, pitchDegrees, up);
			var right = Vector3.Normalize(Vector3.Cross(forward, up));
			var cameraUp = Vector3.Cross(right, forward);
			var backward = -forward;

			// Row vector convention, the inverse of the translation and rotation that place
			// the camera in the world.
			var view = Matrix.Identity;
			view.M11 = right.X;
			view.M12 = cameraUp.X;
			view.M13 = backward.X;
			view.M21 = right.Y;
			view.M22 = cameraUp.Y;
			view.M23 = backward.Y;
			view.M31 = right.Z;
			view.M32 = cameraUp.Z;
			view.M33 = backward.Z;
			view.M41 = -Vector3.Dot(eye, right);
			view.M42 = -Vector3.Dot(eye, cameraUp);
			view.M43 = Vector3.Dot(eye, forward);

			return view;
		}

		/// <summary>
		/// Creates the world space forward direction for the given orientation.
		/// </summary>
		/// <param name="yawDegrees">The yaw in degrees, rotating about <paramref name="up"/>.</param>
		/// <param name="pitchDegrees">The pitch in degrees, positive looking up.</param>
		/// <param name="up">The world up axis.</param>
		/// <returns>The unit forward direction.</returns>
		public static Vector3 CreateForward(float yawDegrees, float pitchDegrees, Vector3 up)
		{
			var yaw = Quaternion.CreateFromAxisAngle(up, MathHelper.ToRadians(yawDegrees));
			var forward = Vector3.Transform(GetReferenceForward(up), yaw);

			var right = Vector3.Normalize(Vector3.Cross(forward, up));
			var pitch = Quaternion.CreateFromAxisAngle(right, MathHelper.ToRadians(pitchDegrees));

			return Vector3.Transform(forward, pitch);
		}

		private static Vector3 GetReferenceForward(Vector3 up)
		{
			// The direction looked at by yaw and pitch of zero, perpendicular to up.
			var reference = Vector3.Forward;
			if (Math.Abs(Vector3.Dot(reference, up)) > 0.999f)
			{
				reference = Vector3.Right;
			}

			reference -= up * Vector3.Dot(reference, up);
			return Vector3.Normalize(reference);
		}

		private void UpdateMovement(float elapsedSeconds)
		{
			var keyboardState = Keyboard.GetState();
			var movement = Vector3.Zero;

			if (keyboardState.IsKeyDown(Keys.W))
				movement += CreateForward(_yaw, _pitch, _up);

			if (keyboardState.IsKeyDown(Keys.S))
				movement -= CreateForward(_yaw, _pitch, _up);

			var right = Vector3.Normalize(Vector3.Cross(CreateForward(_yaw, _pitch, _up), _up));
			if (keyboardState.IsKeyDown(Keys.D))
				movement += right;

			if (keyboardState.IsKeyDown(Keys.A))
				movement -= right;

			if (movement != Vector3.Zero)
			{
				movement.Normalize();
				var speed = MoveSpeed;
				if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
					speed *= SprintMultiplier;

				speed *= MoveSpeedFactor;

				Translate(movement * speed * elapsedSeconds);
			}
		}

		private void UpdateRotation()
		{
			var mouse = Mouse.GetState();
			var mousePosition = new Point(mouse.X, mouse.Y);

			if (mouse.RightButton == ButtonState.Pressed)
			{
				var mouseDelta = _lastMousePosition - mousePosition;

				_yaw += mouseDelta.X * RotationSpeed;
				_pitch = MathHelper.Clamp(_pitch + mouseDelta.Y * RotationSpeed, -MaxPitch, MaxPitch);

				ApplyRotation();
			}

			_lastMousePosition = mousePosition;
		}
	}
}
