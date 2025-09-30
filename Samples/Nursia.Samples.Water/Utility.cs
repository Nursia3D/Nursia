using Microsoft.Xna.Framework;
using System;

namespace Nursia
{
	internal static class Utility
	{
		/// <summary>
		/// The value for which all absolute numbers smaller than are considered equal to zero.
		/// </summary>
		public const float ZeroTolerance = 1e-6f;

		/// <summary>
		/// Compares two floating point numbers based on an epsilon zero tolerance.
		/// </summary>
		/// <param name="left">The first number to compare.</param>
		/// <param name="right">The second number to compare.</param>
		/// <param name="epsilon">The epsilon value to use for zero tolerance.</param>
		/// <returns><c>true</c> if <paramref name="left"/> is within epsilon of <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool EpsilonEquals(this float left, float right, float epsilon = ZeroTolerance)
		{
			return Math.Abs(left - right) <= epsilon;
		}

		public static Quaternion MakeRotationFromTo(Vector3 from, Vector3 to)
		{
			from.Normalize();
			to.Normalize();

			var dot = Vector3.Dot(from, to);
			if (dot.EpsilonEquals(1.0f))
			{
				// Almost identical vectors
				 return Quaternion.Identity;
			}

			Vector3 axis;
			float angle;
			if (dot.EpsilonEquals(-1.0f))
			{
				// Vectors are opposite
				// Choose arbitraty axis perpendecular to from
				axis = Vector3.Cross(from, Vector3.Right);
				if (axis.Length().EpsilonEquals(0.0f))
				{
					// Choose different 2nd vector
					axis = Vector3.Cross(from, Vector3.Up);
				}

				axis.Normalize();

				// 180 degrees
				angle = (float)Math.PI;
			}
			else
			{
				axis = Vector3.Cross(from, to);
				axis.Normalize();

				// Determine angle through ArcCos
				angle = (float)Math.Acos(dot);
			}

			return Quaternion.CreateFromAxisAngle(axis, angle);
		}

		public static Vector3 ToEulerAngles(this Quaternion r)
		{
			return new Vector3
			{
				X = (float)Math.Asin(2.0f * (r.X * r.W - r.Y * r.Z)),
				Y = (float)Math.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y)),
				Z = (float)Math.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z))
			};
		}

		public static Vector3 ToDegrees(this Vector3 v)
		{
			return new Vector3(MathHelper.ToDegrees(v.X), MathHelper.ToDegrees(v.Y), MathHelper.ToDegrees(v.Z));
		}
	}
}
