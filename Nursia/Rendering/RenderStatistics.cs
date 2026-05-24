namespace Nursia.Rendering
{
	/// <summary>
	/// Tracks rendering statistics for performance monitoring and debugging.
	/// </summary>
	public struct RenderStatistics
	{
		/// <summary>
		/// The number of effect/shader switches that occurred during rendering.
		/// </summary>
		public int EffectsSwitches;

		/// <summary>
		/// The number of draw calls made to the graphics device.
		/// </summary>
		public int DrawCalls;

		/// <summary>
		/// The total number of vertices drawn.
		/// </summary>
		public int VerticesDrawn;

		/// <summary>
		/// The total number of primitives (triangles) drawn.
		/// </summary>
		public int PrimitivesDrawn;

		/// <summary>
		/// The number of rendering passes completed.
		/// </summary>
		public int Passes;

		/// <summary>
		/// Resets all statistics counters to zero.
		/// </summary>
		public void Reset()
		{
			EffectsSwitches = 0;
			DrawCalls = 0;
			VerticesDrawn = 0;
			PrimitivesDrawn = 0;
			Passes = 0;
		}
	}
}
