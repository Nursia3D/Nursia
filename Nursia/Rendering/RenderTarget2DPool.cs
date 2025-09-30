using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Rendering
{
	internal class RenderTarget2DPool : GraphicsResourcePool<RenderTarget2D>
	{
		public RenderTarget2D Get(RenderTarget2D existing, string name, int width, int height, bool mipmap = false,
			SurfaceFormat surfaceFormat = SurfaceFormat.Color, DepthFormat depthFormat = DepthFormat.None,
			int multisampleCount = 0, RenderTargetUsage usage = RenderTargetUsage.DiscardContents)
		{
			if (existing != null &&
				existing.Width == width &&
				existing.Height == height &&
				existing.LevelCount > 1 == mipmap &&
				existing.Format == surfaceFormat &&
				existing.DepthStencilFormat == depthFormat &&
				existing.MultiSampleCount == multisampleCount &&
				existing.RenderTargetUsage == usage)
			{
				// Existing has required parameters
				return existing;
			}

			// Recycle existing
			if (existing != null)
			{
				Recycle(existing);
			}

			// Check if we have such render target in the storage
			// Search from end to beginning
			// Since we add recycled targets to the end
			int? index = null;
			for (var i = _storage.Count - 1; i >= 0; --i)
			{
				var storedTarget = _storage[i];

				if (storedTarget.Width == width &&
					storedTarget.Height == height &&
					storedTarget.LevelCount > 1 == mipmap &&
					storedTarget.Format == surfaceFormat &&
					storedTarget.DepthStencilFormat == depthFormat &&
					storedTarget.MultiSampleCount == multisampleCount &&
					storedTarget.RenderTargetUsage == usage)
				{
					// Found
					index = i;
					break;
				}
			}

			if (index != null)
			{
				// Use stored
				return GetFromStorage(index.Value);
			}

			// Create new one
			var newTarget = new RenderTarget2D(Nrs.GraphicsDevice, width, height, mipmap, surfaceFormat, depthFormat, multisampleCount, usage);
			OnTargetCreated($"{name}:{width}x{height}", newTarget);

			return newTarget;
		}
	}
}
