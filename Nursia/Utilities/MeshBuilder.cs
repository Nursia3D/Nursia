using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Nursia.Utilities
{
	internal class MeshBuilder
	{
		private bool _uses32BitIndices = false;

		public List<VertexPositionNormalTexture> Vertices = new List<VertexPositionNormalTexture>();
		private List<int> _indices { get; } = new List<int>();

		public IReadOnlyList<int> Indices => _indices;
		public int LastIndex => _indices[_indices.Count - 1];

		public void AddVertex(VertexPositionNormalTexture v) => Vertices.Add(v);

		public void AddIndex(int index)
		{
			if (index >= ushort.MaxValue)
			{
				_uses32BitIndices = true;
			}

			_indices.Add(index);
		}

		public void AddIndicesRange(IEnumerable<int> indices)
		{
			foreach (var idx in indices)
			{
				AddIndex(idx);
			}
		}

		public int GetIndex(int index) => _indices[index];
		public int[] CreateIndicesArray() => _indices.ToArray();

		public void ClearIndices()
		{
			_indices.Clear();
			_uses32BitIndices = false;
		}

		public DrMeshPart CreateMeshPart(GraphicsDevice graphicsDevice)
		{
			IndexBuffer indexBuffer;
			if (!_uses32BitIndices)
			{
				var indicesShort = new ushort[_indices.Count];
				for (var i = 0; i < indicesShort.Length; ++i)
				{
					indicesShort[i] = (ushort)_indices[i];
				}

				indexBuffer = indicesShort.CreateIndexBuffer(graphicsDevice);
			}
			else
			{
				indexBuffer = _indices.ToArray().CreateIndexBuffer(graphicsDevice);
			}

			var vertexBuffer = Vertices.ToArray().CreateVertexBuffer(graphicsDevice);


			return new DrMeshPart(vertexBuffer, indexBuffer, BoundingBox.CreateFromPoints(from v in Vertices select v.Position));
		}
	}
}
