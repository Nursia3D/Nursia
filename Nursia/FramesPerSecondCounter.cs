using System;

namespace Nursia
{
	public class FramesPerSecondCounter
	{
		private int _framesCounter;
		private DateTime _lastTime = DateTime.Now;

		public FramesPerSecondCounter()
		{
		}

		public int FramesPerSecond { get; private set; }

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