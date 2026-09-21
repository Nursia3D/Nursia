using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nursia.SceneGraph;

namespace Nursia.Utilities
{
	/// <summary>
	/// Handles keyboard and mouse input for controlling a camera in real-time.
	/// </summary>
	public class CameraInputController
	{
		private Point _lastMousePosition;

		/// <summary>
		/// Gets the camera being controlled.
		/// </summary>
		public Camera Camera { get; }

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
		/// Gets or sets the point to keep the movement speed in proportion to.
		/// When set, the movement speed scales with the distance from the camera
		/// to this point, so far away objects are approached quickly.
		/// </summary>
		public Vector3? FocusPoint { get; set; }

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

		private void UpdateMovement(float elapsedSeconds)
		{
			var keyboardState = Keyboard.GetState();
			var movement = Vector3.Zero;
			var transform = Camera.GlobalTransform;

			if (keyboardState.IsKeyDown(Keys.W))
				movement += transform.Forward;

			if (keyboardState.IsKeyDown(Keys.S))
				movement -= transform.Forward;

			if (keyboardState.IsKeyDown(Keys.D))
				movement += transform.Right;

			if (keyboardState.IsKeyDown(Keys.A))
				movement -= transform.Right;

			if (movement != Vector3.Zero)
			{
				movement.Normalize();
				var speed = MoveSpeed;
				if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
					speed *= SprintMultiplier;

				if (FocusPoint != null)
				{
					var distance = (transform.Translation - FocusPoint.Value).Length();
					speed *= distance;
				}

				speed *= MoveSpeedFactor;

				Camera.Translation += movement * speed * elapsedSeconds;
			}
		}

		private void UpdateRotation()
		{
			var mouse = Mouse.GetState();
			var mousePosition = new Point(mouse.X, mouse.Y);

			if (mouse.RightButton == ButtonState.Pressed)
			{
				var mouseDelta = _lastMousePosition - mousePosition;

				var rotation = Camera.Rotation;
				rotation.X += mouseDelta.Y * RotationSpeed;
				rotation.Y += mouseDelta.X * RotationSpeed;

				Camera.Rotation = rotation;
			}

			_lastMousePosition = mousePosition;
		}
	}
}
