using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace DigitalRiseModel.Samples.Character
{
	/// <summary>
	/// Event args for input value changes, containing the old and new values.
	/// </summary>
	public class InputEventArgs<T> : EventArgs
	{
		/// <summary>Gets the previous value.</summary>
		public T OldValue { get; }

		/// <summary>Gets the new value.</summary>
		public T NewValue { get; }

		/// <summary>
		/// Initializes a new instance of the InputEventArgs class.
		/// </summary>
		public InputEventArgs(T oldValue, T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	/// <summary>
	/// Event args for keyboard input events.
	/// </summary>
	public class KeyEventsArgs : EventArgs
	{
		/// <summary>Gets the key that was pressed or released.</summary>
		public Keys Key { get; }

		/// <summary>
		/// Initializes a new instance of the KeyEventsArgs class.
		/// </summary>
		public KeyEventsArgs(Keys key)
		{
			Key = key;
		}
	}

	/// <summary>Manages keyboard and mouse input, providing state change events.</summary>
	public class InputService
	{
		private readonly bool[] _keysDown = new bool[256];

		/// <summary>Current mouse position in screen coordinates.</summary>
		public Point? MousePosition { get; private set; }

		/// <summary>Whether the mouse is locked (confined and hidden).</summary>
		public bool MouseLocked
		{
			get => !ViewerGame.Instance.IsMouseVisible;

			set
			{
				if (value == MouseLocked)
					return;

				ViewerGame.Instance.IsMouseVisible = !value;
				MousePosition = null;
				UpdateLockedMouse();
			}
		}

		/// <summary>
		/// Raised when the mouse moves. Event args contain old and new mouse positions.
		/// </summary>
		public event EventHandler<InputEventArgs<Point>> MouseMoved;

		/// <summary>
		/// Raised when a key is pressed (transition from not-pressed to pressed).
		/// </summary>
		public event EventHandler<KeyEventsArgs> KeyDown;

		/// <summary>
		/// Raised when a key is released (transition from pressed to not-pressed).
		/// </summary>
		public event EventHandler<KeyEventsArgs> KeyUp;

		/// <summary>
		/// Checks if a specific key is currently pressed.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>true if the key is currently pressed; false otherwise.</returns>
		public bool IsKeyDown(Keys key) => _keysDown[(int)key];

		/// <summary>Updates input states and raises events. Call once per frame.</summary>
		public void Update()
		{
			if (!ViewerGame.Instance.IsActive)
			{
				MousePosition = null;
				return;
			}

			var mouseState = Mouse.GetState();
			var newPosition = new Point(mouseState.X, mouseState.Y);

			if (MousePosition == null)
			{
				MousePosition = newPosition;
			}
			else
			{
				var oldPosition = MousePosition.Value;
				MousePosition = newPosition;

				if (!MouseLocked)
				{
					if (newPosition != oldPosition)
						MouseMoved?.Invoke(this, new InputEventArgs<Point>(oldPosition, newPosition));
				}
				else
				{
					var cb = ViewerGame.Instance.Window.ClientBounds;
					var center = new Point(cb.Width / 2, cb.Height / 2);

					if (newPosition != center)
						MouseMoved?.Invoke(this, new InputEventArgs<Point>(center, newPosition));
				}
			}

			UpdateLockedMouse();

			var keyboardState = Keyboard.GetState();
			for (var i = 0; i < _keysDown.Length; ++i)
			{
				var key = (Keys)i;
				var isDown = keyboardState.IsKeyDown(key);

				if (!_keysDown[i] && isDown)
					KeyDown?.Invoke(this, new KeyEventsArgs(key));
				else if (_keysDown[i] && !isDown)
					KeyUp?.Invoke(this, new KeyEventsArgs(key));

				_keysDown[i] = isDown;
			}
		}

		/// <summary>Centers mouse at window center when locked.</summary>
		private void UpdateLockedMouse()
		{
			if (!MouseLocked)
				return;

			var cb = ViewerGame.Instance.Window.ClientBounds;
			var center = new Point(cb.Width / 2, cb.Height / 2);
			Mouse.SetPosition(center.X, center.Y);
		}
	}
}
