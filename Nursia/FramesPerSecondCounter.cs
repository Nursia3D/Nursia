using System;

namespace Nursia
{
	/// <summary>
	/// Tracks and calculates frames per second for performance monitoring.
	/// </summary>
	public class FramesPerSecondCounter
	{
		private int _framesCounter;
		private DateTime _lastTime = DateTime.Now;

		/// <summary>
		/// Initializes a new instance of the <see cref="FramesPerSecondCounter"/> class.
		/// </summary>
		public FramesPerSecondCounter()
		{
		}

		/// <summary>
		/// Gets the current frames per second measurement.
		/// </summary>
		public int FramesPerSecond { get; private set; }

		/// <summary>
		/// Called when a frame has been drawn to update the FPS counter.
		/// </summary>
		public void OnFrameDrawn()
		{
			if ((DateTime.Now - _lastTime).TotalSeconds < 1)
			{
				_framesCounter++;
			}
			else
			{
				// 1 second passed
				FramesPerSecond = _framesCounter;
				_framesCounter = 0;
				_lastTime = DateTime.Now;
			}
		}
	}
}