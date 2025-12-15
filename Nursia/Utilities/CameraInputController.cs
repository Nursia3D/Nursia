// This code was borrowed from https://github.com/DigitalRune/DigitalRune

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nursia.SceneGraph;
using System;

namespace Nursia.Utilities
{
	public class CameraInputController
	{
		private const float LinearVelocityMagnitude = 5f;
		private const float AngularVelocityMagnitude = 0.1f;

		// Position and Orientation of camera.
		private Vector3 _defaultPosition = new Vector3(0, 2, 5);
		private float _defaultYaw;
		private float _defaultPitch;
		private float _currentYaw;
		private float _currentPitch;

		private DateTime? _lastDateTime;
		private KeyboardState? _lastKeybordState;
		private MouseState? _lastMouseState;


		// This property is null while the CameraObject is not added to the game
		// object service.
		public Camera Camera { get; private set; }

		public bool IsEnabled { get; set; }

		public float SpeedBoost { get; set; } = 20.0f;


		public CameraInputController(Camera camera)
		{
			Camera = camera ?? throw new ArgumentNullException(nameof(camera));
			IsEnabled = true;

			_currentPitch = MathHelper.ToRadians(camera.Rotation.X);
			_currentYaw = MathHelper.ToRadians(camera.Rotation.Y);
		}


		public void ResetPose(Vector3 position, float yaw, float pitch)
		{
			_defaultPosition = position;
			_defaultYaw = yaw;
			_defaultPitch = pitch;

			ResetPose();
		}

		public void ResetPose()
		{
			_currentYaw = _defaultYaw;
			_currentPitch = _defaultPitch;

			// Also update SceneNode.LastPose - this is required for some effect, like
			// object motion blur. 
			// CameraNode.SetLastPose(true);
			Camera.Translation = _defaultPosition;
			Camera.Rotation = new Vector3(MathHelper.ToDegrees(_currentPitch), MathHelper.ToDegrees(_currentYaw), 0);
		}

		public void Update()
		{
			if (!IsEnabled)
			{
				return;
			}

			if (_lastDateTime == null)
			{
				_lastDateTime = DateTime.Now;
				return;
			}

			float deltaTimeF = (float)(DateTime.Now - _lastDateTime.Value).TotalSeconds;
			_lastDateTime = DateTime.Now;

			// Compute new orientation from mouse movement, gamepad and touch.
			var mouseState = Mouse.GetState();

			var mousePositionDelta = Vector2.Zero;

			if (mouseState.RightButton == ButtonState.Pressed)
			{
				if (_lastMouseState != null)
				{
					mousePositionDelta = new Vector2(mouseState.X - _lastMouseState.Value.X,
						mouseState.Y - _lastMouseState.Value.Y);
				}

				_lastMouseState = mouseState;
			}
			else
			{
				_lastMouseState = null;
			}

			float deltaYaw = -mousePositionDelta.X;
			_currentYaw += deltaYaw * deltaTimeF * AngularVelocityMagnitude;

			float deltaPitch = -mousePositionDelta.Y;
			_currentPitch += deltaPitch * deltaTimeF * AngularVelocityMagnitude;

			// Reset camera position if <Home> or <Right Stick> is pressed.
			var keyboardState = Keyboard.GetState();

			if (_lastKeybordState != null && keyboardState.IsKeyDown(Keys.Home) &&
				!_lastKeybordState.Value.IsKeyDown(Keys.Home))
			{
				ResetPose();
			}

			_lastKeybordState = keyboardState;

			// Compute new orientation of the camera.
			var orientation = Mathematics.CreateFromAxisY(_currentYaw) * Mathematics.CreateFromAxisX(_currentPitch);

			// Create velocity from <W>, <A>, <S>, <D> and <R>, <F> keys. 
			// <R> or DPad up is used to move up ("rise"). 
			// <F> or DPad down is used to move down ("fall").
			Vector3 velocity = Vector3.Zero;
			if (keyboardState.IsKeyDown(Keys.W))
				velocity.Z--;
			if (keyboardState.IsKeyDown(Keys.S))
				velocity.Z++;
			if (keyboardState.IsKeyDown(Keys.A))
				velocity.X--;
			if (keyboardState.IsKeyDown(Keys.D))
				velocity.X++;
			if (keyboardState.IsKeyDown(Keys.R))
				velocity.Y++;
			if (keyboardState.IsKeyDown(Keys.F))
				velocity.Y--;

			// Rotate the velocity vector from view space to world space.
			velocity = Vector3.Transform(velocity, orientation);

			if (keyboardState.IsKeyDown(Keys.LeftShift))
				velocity *= SpeedBoost;

			// Multiply the velocity by time to get the translation for this frame.
			Vector3 translation = velocity * LinearVelocityMagnitude * deltaTimeF;

			// Update SceneNode.LastPoseWorld - this is required for some effects, like
			// camera motion blur. 
			// Camera.LastPoseWorld = Camera.PoseWorld;

			// Set the new camera pose.
			Camera.Rotation = new Vector3(MathHelper.ToDegrees(_currentPitch),
				MathHelper.ToDegrees(_currentYaw),
				0);

			Camera.Translation += translation;
		}
	}
}