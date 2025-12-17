using DigitalRiseModel;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nursia.Utilities
{
	internal static class XNA
	{
		public static int GetSize(this VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 4;
				case VertexElementFormat.Vector2:
					return 8;
				case VertexElementFormat.Vector3:
					return 12;
				case VertexElementFormat.Vector4:
					return 16;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 4;
				case VertexElementFormat.Short4:
					return 8;
				case VertexElementFormat.NormalizedShort2:
					return 4;
				case VertexElementFormat.NormalizedShort4:
					return 8;
				case VertexElementFormat.HalfVector2:
					return 4;
				case VertexElementFormat.HalfVector4:
					return 8;
			}

			return 0;
		}

		public static EffectParameter FindParameterByName(this Effect effect, string name)
		{
			foreach (var par in effect.Parameters)
			{
				if (par.Name == name)
				{
					return par;
				}
			}

			return null;
		}

		public static VertexBuffer CreateVertexBuffer<T>(this T[] vertices, GraphicsDevice device) where T : struct, IVertexType
		{
			var result = new VertexBuffer(device, new T().VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			result.SetData(vertices);

			return result;
		}

		public static IndexBuffer CreateIndexBuffer(this ushort[] indices, GraphicsDevice device)
		{
			var result = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
			result.SetData(indices);

			return result;
		}

		public static IndexBuffer CreateIndexBuffer(this int[] indices, GraphicsDevice device)
		{
			var result = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
			result.SetData(indices);

			return result;
		}

		public static void ResetTextures(this GraphicsDevice device)
		{
			for (var i = 0; i < 16; ++i)
			{
				device.Textures[i] = null;
			}
		}

		public static void Draw(this DrMeshPart mesh, GraphicsDevice graphicsDevice, VertexBuffer instancesTransforms)
		{
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException(nameof(graphicsDevice));
			}

			if (instancesTransforms == null)
			{
				graphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
				if (mesh.IndexBuffer == null)
				{
					graphicsDevice.DrawPrimitives(mesh.PrimitiveType, mesh.VertexOffset, mesh.PrimitiveCount);
				}
				else
				{

					graphicsDevice.Indices = mesh.IndexBuffer;

#if MONOGAME
					graphicsDevice.DrawIndexedPrimitives(mesh.PrimitiveType, mesh.VertexOffset, mesh.StartIndex, mesh.PrimitiveCount);
#else
					graphicsDevice.DrawIndexedPrimitives(mesh.PrimitiveType, mesh.VertexOffset, 0, mesh.NumVertices, mesh.StartIndex, mesh.PrimitiveCount);
#endif
				}
			}
			else
			{
				graphicsDevice.SetVertexBuffers(new VertexBufferBinding(mesh.VertexBuffer), new VertexBufferBinding(instancesTransforms, 0, 1));

				if (mesh.IndexBuffer == null)
				{
					throw new Exception($"Instanced primitives must be indexed");
				}
				else
				{

					graphicsDevice.Indices = mesh.IndexBuffer;

#if MONOGAME
					graphicsDevice.DrawInstancedPrimitives(mesh.PrimitiveType, mesh.VertexOffset, mesh.StartIndex, mesh.PrimitiveCount, instancesTransforms.VertexCount);
#else
					graphicsDevice.DrawInstancedPrimitives(mesh.PrimitiveType, mesh.VertexOffset, 0, mesh.NumVertices, mesh.StartIndex, mesh.PrimitiveCount, instancesTransforms.VertexCount);
#endif
				}
			}
		}
	}
}
