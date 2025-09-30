// This code had been borrowed from https://github.com/gameplay3d/gameplay
// And ported to C#

using Microsoft.Xna.Framework;
using Nursia.Utilities;
using StbImageSharp;
using System;
using System.Diagnostics;
using System.IO;

namespace Nursia.SceneGraph.Landscape
{
	/// <summary>
	/// Defines height data used to store values representing elevation.
	/// </summary>
	public class HeightField
	{
		private readonly float[,] _array;

		public int Columns => _array.GetLength(0);
		public int Rows => _array.GetLength(1);

		public Action<Point> HeightChanged;

		/// <summary>
		/// Creates a new HeightField of the given dimensions, with zero height data.
		/// </summary>
		/// <param name="rows"></param>
		/// <param name="columns"></param>
		public HeightField(int columns, int rows)
		{
			if (columns < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(columns), "columns < 2");
			}

			if (rows < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(rows), "rows < 2");
			}

			_array = new float[columns, rows];
		}

		public void SetHeight(int column, int row, float value)
		{
			if (value.EpsilonEquals(_array[column, row]))
			{
				return;
			}

			_array[column, row] = value;
			HeightChanged?.Invoke(new Point(column, row));
		}

		public float GetHeight(int column, int row) => _array[column, row];

		/// <summary>
		/// Returns the height at the specified row and column.
		/// </summary>
		/// <param name="col"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		public float CalculateInterpolatedHeight(float column, float row)
		{
			// Clamp to heightfield boundaries
			column = column < 0 ? 0 : column > Columns - 1 ? Columns - 1 : column;
			row = row < 0 ? 0 : row > Rows - 1 ? Rows - 1 : row;

			int x1 = (int)column;
			int y1 = (int)row;
			int x2 = x1 + 1;
			int y2 = y1 + 1;
			float xFactor = column - x1;
			float yFactor = row - y1;
			float xFactorI = 1.0f - xFactor;
			float yFactorI = 1.0f - yFactor;

			if (x2 >= Columns && y2 >= Rows)
			{
				return _array[x1, y1];
			}
			else if (x2 >= Columns)
			{
				return _array[x1, y1] * yFactorI + _array[x1, y2] * yFactor;
			}
			else if (y2 >= Rows)
			{
				return _array[x1, y1] * xFactorI + _array[x2, y1] * xFactor;
			}
			else
			{
				float a = xFactorI * yFactorI;
				float b = xFactorI * yFactor;
				float c = xFactor * yFactor;
				float d = xFactor * yFactorI;
				return _array[x1, y1] * a + _array[x1, y2] * b + _array[x2, y2] * c + _array[x2, y1] * d;
			}
		}

		public static HeightField FromStreamR8(Stream stream, int columns, int rows, float heightMin = 0.0f, float heightMax = 1.0f, bool revertX = false, bool revertZ = false)
		{
			Debug.Assert(heightMax >= heightMin);

			var heightScale = heightMax - heightMin;
			var result = new HeightField(columns, rows);

			for (var y = 0; y < rows; ++y)
			{
				for (var x = 0; x < columns; ++x)
				{
					var b1 = stream.ReadByte();
					var value = heightMin + b1 / 255.0f * heightScale;

					var dx = x;
					var dy = y;

					if (revertX)
					{
						dx = columns - 1 - x;
					}

					if (revertZ)
					{
						dy = rows - 1 - y;
					}
					
					result.SetHeight(dx, dy, value);
				}
			}

			return result;
		}

		public static HeightField FromStreamR16(Stream stream, int columns, int rows, float heightMin = 0.0f, float heightMax = 1.0f)
		{
			Debug.Assert(heightMax >= heightMin);

			var heightScale = heightMax - heightMin;
			var result = new HeightField(columns, rows);

			// 16-bit (0-65535)
			for (var y = 0; y < rows; ++y)
			{
				for (var x = 0; x < columns; ++x)
				{
					var b1 = stream.ReadByte();
					var b2 = stream.ReadByte();
					var value = heightMin + (b1 | b2 << 8) / 65535.0f * heightScale;
					result.SetHeight(x, y, value);
				}
			}

			return result;
		}

		public static HeightField FromStreamImage(Stream stream, float heightMin = 0.0f, float heightMax = 1.0f)
		{
			var imageResult = ImageResult.FromStream(stream);

			if (imageResult.Comp != ColorComponents.Grey)
			{
				throw new NotSupportedException($"Only single channel images are supported");
			}

			var columns = imageResult.Width;
			var rows = imageResult.Height;
			var heightScale = heightMax - heightMin;

			var result = new HeightField(columns, rows);

			// 16-bit (0-65535)
			for (var y = 0; y < rows; ++y)
			{
				for (var x = 0; x < columns; ++x)
				{
					var b = imageResult.Data[x + y * columns];
					var value = heightMin + b * heightScale / 255.0f;
					result.SetHeight(x, y, value);
				}
			}

			return result;
		}

		public static HeightField FromBytesR16(byte[] bytes, int columns, int rows, float heightMin = 0.0f, float heightMax = 1.0f)
		{
			using (var ms = new MemoryStream(bytes))
			{
				return FromStreamR16(ms, columns, rows, heightMin, heightMax);
			}
		}

		public static HeightField FromStreamHf(Stream stream)
		{
			HeightField result;
			using (var reader = new BinaryReader(stream))
			{
				var columns = reader.ReadInt32();
				var rows = reader.ReadInt32();

				result = new HeightField(columns, rows);
				for (var y = 0; y < rows; ++y)
				{
					for (var x = 0; x < columns; ++x)
					{
						var value = reader.ReadSingle();
						result.SetHeight(x, y, value);
					}
				}
			}

			return result;
		}

		public static HeightField FromHfBytes(byte[] bytes)
		{
			using (var ms = new MemoryStream(bytes))
			{
				return FromStreamHf(ms);
			}
		}

		public void SaveToHf(Stream stream)
		{
			using (var output = new BinaryWriter(stream))
			{
				// Size
				output.Write(Columns);
				output.Write(Rows);

				// Data
				for (var y = 0; y < Rows; ++y)
				{
					for (var x = 0; x < Columns; ++x)
					{
						output.Write(GetHeight(x, y));
					}
				}
			}
		}
	}
}
