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
	/// Defines height data used to store values representing elevation in a heightfield.
	/// </summary>
	public class HeightField
	{
		private readonly float[,] _array;

		/// <summary>
		/// Gets the number of columns in this height field.
		/// </summary>
		public int Columns => _array.GetLength(0);

		/// <summary>
		/// Gets the number of rows in this height field.
		/// </summary>
		public int Rows => _array.GetLength(1);

		/// <summary>
		/// Gets or sets an action that is called when a height value changes.
		/// </summary>
		public Action<Point> HeightChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="HeightField"/> class with the given dimensions filled with zero height data.
		/// </summary>
		/// <param name="columns">The number of columns. Must be at least 2.</param>
		/// <param name="rows">The number of rows. Must be at least 2.</param>
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

		/// <summary>
		/// Sets the height value at the specified position.
		/// </summary>
		/// <param name="column">The column index.</param>
		/// <param name="row">The row index.</param>
		/// <param name="value">The height value to set.</param>
		public void SetHeight(int column, int row, float value)
		{
			if (value.EpsilonEquals(_array[column, row]))
			{
				return;
			}

			_array[column, row] = value;
			HeightChanged?.Invoke(new Point(column, row));
		}

		/// <summary>
		/// Gets the height value at the specified column and row.
		/// </summary>
		/// <param name="column">The column index.</param>
		/// <param name="row">The row index.</param>
		/// <returns>The height value at the specified position.</returns>
		public float GetHeight(int column, int row) => _array[column, row];

		/// <summary>
		/// Calculates the interpolated height at a float position using bilinear interpolation.
		/// </summary>
		/// <param name="column">The column index (supports fractional values).</param>
		/// <param name="row">The row index (supports fractional values).</param>
		/// <returns>The interpolated height value.</returns>
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

		/// <summary>
		/// Loads a height field from an 8-bit raw stream.
		/// </summary>
		/// <param name="stream">The stream containing 8-bit height values.</param>
		/// <param name="columns">The number of columns in the height field.</param>
		/// <param name="rows">The number of rows in the height field.</param>
		/// <param name="heightMin">The minimum height value to map to.</param>
		/// <param name="heightMax">The maximum height value to map to.</param>
		/// <param name="revertX">Whether to flip the X axis.</param>
		/// <param name="revertZ">Whether to flip the Z axis.</param>
		/// <returns>A new HeightField loaded from the stream.</returns>
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

		/// <summary>
		/// Loads a height field from a 16-bit raw stream.
		/// </summary>
		/// <param name="stream">The stream containing 16-bit height values in little-endian format.</param>
		/// <param name="columns">The number of columns in the height field.</param>
		/// <param name="rows">The number of rows in the height field.</param>
		/// <param name="heightMin">The minimum height value to map to.</param>
		/// <param name="heightMax">The maximum height value to map to.</param>
		/// <returns>A new HeightField loaded from the stream.</returns>
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

		/// <summary>
		/// Loads a height field from an image stream. Supports single-channel grayscale images.
		/// </summary>
		/// <param name="stream">The image stream containing height data.</param>
		/// <param name="heightMin">The minimum height value to map to.</param>
		/// <param name="heightMax">The maximum height value to map to.</param>
		/// <returns>A new HeightField loaded from the image.</returns>
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

		/// <summary>
		/// Loads a height field from 16-bit raw bytes.
		/// </summary>
		/// <param name="bytes">The byte array containing 16-bit height values.</param>
		/// <param name="columns">The number of columns in the height field.</param>
		/// <param name="rows">The number of rows in the height field.</param>
		/// <param name="heightMin">The minimum height value to map to.</param>
		/// <param name="heightMax">The maximum height value to map to.</param>
		/// <returns>A new HeightField loaded from the bytes.</returns>
		public static HeightField FromBytesR16(byte[] bytes, int columns, int rows, float heightMin = 0.0f, float heightMax = 1.0f)
		{
			using (var ms = new MemoryStream(bytes))
			{
				return FromStreamR16(ms, columns, rows, heightMin, heightMax);
			}
		}

		/// <summary>
		/// Loads a height field from a Nursia HF format stream.
		/// </summary>
		/// <param name="stream">The stream containing HF format height data.</param>
		/// <returns>A new HeightField loaded from the stream.</returns>
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

		/// <summary>
		/// Loads a height field from HF format bytes.
		/// </summary>
		/// <param name="bytes">The byte array containing HF format height data.</param>
		/// <returns>A new HeightField loaded from the bytes.</returns>
		public static HeightField FromHfBytes(byte[] bytes)
		{
			using (var ms = new MemoryStream(bytes))
			{
				return FromStreamHf(ms);
			}
		}

		/// <summary>
		/// Saves this height field to a stream in HF format.
		/// </summary>
		/// <param name="stream">The stream to save the height field to.</param>
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
