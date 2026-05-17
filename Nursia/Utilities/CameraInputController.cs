using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nursia.SceneGraph;

namespace Nursia.Utilities
{
	public class CameraInputController
	{
		private Point _lastMousePosition;

		public Camera Camera { get; }
		public float MoveSpeed { get; set; } = 10.0f;
		public float RotationSpeed { get; set; } = 0.1f;
		public float SprintMultiplier { get; set; } = 2.0f;

		public CameraInputController(Camera camera)
		{
			Camera = camera;
			_lastMousePosition = Mouse.GetState().Position;
		}

		public void Update()
		{
			UpdateMovement();
			UpdateRotation();
		}

		private void UpdateMovement()
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

				Camera.Translation += movement * speed * 0.016f;
			}
		}

		private void UpdateRotation()
		{
			var mouseState = Mouse.GetState();

			if (mouseState.RightButton == ButtonState.Pressed)
			{
				var mouseDelta = _lastMousePosition - mouseState.Position;

				var rotation = Camera.Rotation;
				rotation.X += mouseDelta.Y * RotationSpeed;
				rotation.Y += mouseDelta.X * RotationSpeed;

				Camera.Rotation = rotation;
			}

			_lastMousePosition = mouseState.Position;
		}

	}
}
