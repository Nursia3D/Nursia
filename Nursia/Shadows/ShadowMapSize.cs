namespace Nursia.Shadows
{
	public enum ShadowMapSize
	{
		Size1024,
		Size2048,
		Size4096,
		Size8192,
		Size16384
	}

	internal static class ShadowCascadeSizeExtensions
	{
		private static readonly int[] _sizes = new[] { 1024, 2048, 4096, 8192, 16384 };

		public static int GetSize(this ShadowMapSize size) => _sizes[(int)size];
	}
}
