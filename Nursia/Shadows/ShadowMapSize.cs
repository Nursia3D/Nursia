namespace Nursia.Shadows
{
	/// <summary>
	/// Specifies the resolution of shadow maps.
	/// </summary>
	public enum ShadowMapSize
	{
		/// <summary>1024x1024 shadow map resolution.</summary>
		Size1024,

		/// <summary>2048x2048 shadow map resolution.</summary>
		Size2048,

		/// <summary>4096x4096 shadow map resolution.</summary>
		Size4096,

		/// <summary>8192x8192 shadow map resolution.</summary>
		Size8192,

		/// <summary>16384x16384 shadow map resolution.</summary>
		Size16384
	}

	internal static class ShadowCascadeSizeExtensions
	{
		private static readonly int[] _sizes = new[] { 1024, 2048, 4096, 8192, 16384 };

		public static int GetSize(this ShadowMapSize size) => _sizes[(int)size];
	}
}
