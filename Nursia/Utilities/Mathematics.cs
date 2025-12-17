using Microsoft.Xna.Framework;
using Nursia.SceneGraph;
using System;

namespace Nursia.Utilities
{
	internal static class Mathematics
	{
		public static readonly BoundingBox DefaultBox = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f));

		private readonly static Vector3[] _corners = new Vector3[8];

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

		public static bool EpsilonEquals(this Vector2 a, Vector2 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon);
		}

		public static bool EpsilonEquals(this Vector3 a, Vector3 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon);
		}

		public static bool EpsilonEquals(this Vector4 a, Vector4 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon) &&
				a.W.EpsilonEquals(b.W, epsilon);
		}

		public static bool IsZero(this float a)
		{
			return a.EpsilonEquals(0.0f);
		}

		public static bool IsZero(this Vector2 a)
		{
			return a.EpsilonEquals(Vector2.Zero);
		}

		public static bool IsZero(this Vector3 a)
		{
			return a.EpsilonEquals(Vector3.Zero);
		}

		public static bool IsZero(this Vector4 a)
		{
			return a.EpsilonEquals(Vector4.Zero);
		}

		public static Plane CreatePlane(float height, Vector3 planeNormalDirection, Matrix viewProjection, bool clipSide)
		{
			planeNormalDirection.Normalize();
			Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
			if (clipSide)
				planeCoeffs *= -1;

			Matrix inverseWorldViewProjection = Matrix.Invert(viewProjection);
			inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

			planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);
			Plane finalPlane = new Plane(planeCoeffs);

			return finalPlane;
		}

		public static Vector3 ToVector3(this float[] array) => new Vector3(array[0], array[1], array[2]);
		public static Vector4 ToVector4(this float[] array) => new Vector4(array[0], array[1], array[2], array[3]);
		public static Quaternion ToQuaternion(this float[] array) => new Quaternion(array[0], array[1], array[2], array[3]);

		public static Matrix ToMatrix(this float[] array) =>
			new Matrix(array[0], array[1], array[2], array[3],
				array[4], array[5], array[6], array[7],
				array[8], array[9], array[10], array[11],
				array[12], array[13], array[14], array[15]);

		public static BoundingBox Transform(this BoundingBox source, ref Matrix matrix)
		{
			source.GetCorners(_corners);

			for (var i = 0; i < _corners.Length; ++i)
			{
				Vector3.Transform(ref _corners[i], ref matrix, out Vector3 v);
				_corners[i] = v;

			}
			return BoundingBox.CreateFromPoints(_corners);
		}

		public static Vector3 CalculateCenter(this BoundingBox source)
		{
			return new Vector3(
				source.Min.X + (source.Max.X - source.Min.X) / 2,
				source.Min.Y + (source.Max.Y - source.Min.Y) / 2,
				source.Min.Z + (source.Max.Z - source.Min.Z) / 2);
		}

		public static bool IsEmpty(this BoundingBox source)
		{
			var d = source.Max - source.Min;

			return d.X.IsZero() && d.Y.IsZero() && d.Z.IsZero();
		}

		public static Vector4 Lerp(Vector4 a, Vector4 b, float s)
		{
			return new Vector4(MathHelper.Lerp(a.X, b.X, s),
				MathHelper.Lerp(a.Y, b.Y, s),
				MathHelper.Lerp(a.Z, b.Z, s),
				MathHelper.Lerp(a.W, b.W, s));
		}

		public static Color ToColor(this Vector4 v)
		{
			return new Color((byte)(v.X * 255.0f), (byte)(v.Y * 255.0f), (byte)(v.Z * 255.0f), (byte)(v.W * 255.0f));
		}

		public static float ClampDegree(this float deg)
		{
			var isNegative = deg < 0;
			deg = Math.Abs(deg);
			deg = deg % 360;
			if (isNegative)
			{
				deg = 360 - deg;
			}

			return deg;
		}

		public static Quaternion CreateFromAxisX(float angle) => Quaternion.CreateFromAxisAngle(Vector3.UnitX, angle);
		public static Quaternion CreateFromAxisY(float angle) => Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
		public static Quaternion CreateFromAxisZ(float angle) => Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);


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
			return new Vector3
			{
				X = MathHelper.ToDegrees(v.X),
				Y = MathHelper.ToDegrees(v.Y),
				Z = MathHelper.ToDegrees(v.Z),
			};
		}

		public static Matrix CreateScreenAlignedBillboard(Matrix view, Vector3 objectPosition)
		{
			var result = Matrix.Invert(view);
			result.Translation = objectPosition;

			return result;
		}

		public static void SetScale(this ref Matrix matrix, Vector3 scale)
		{
			matrix.M11 = scale.X;
			matrix.M22 = scale.Y;
			matrix.M33 = scale.Z;
		}

		public static int Clamp(this int val, int min, int max)
		{
			if (val < min)
			{
				val = min;
			}

			if (val > max)
			{
				val = max;
			}

			return val;
		}

		public static Vector4 ToVector4(this Plane p) => new Vector4(p.Normal, p.D);

		public static bool IsZero(this Plane p) => p.Normal.IsZero();

		public static void UpdateLightCamera(BoundingFrustum cameraFrustum, Vector3 lightDirection, Camera result)
		{
			// Matrix with that will rotate in points the direction of the light
			Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
													   lightDirection,
													   Vector3.Up);

			// Get the corners of the frustum
			// Limit the camera far plane
			cameraFrustum.GetCorners(_corners);

			// Transform the positions of the corners into the direction of the light
			for (int i = 0; i < _corners.Length; i++)
			{
				_corners[i] = Vector3.Transform(_corners[i], lightRotation);
			}

			// Find the smallest box around the points
			BoundingBox lightBox = BoundingBox.CreateFromPoints(_corners);

			Vector3 boxSize = lightBox.Max - lightBox.Min;
			Vector3 halfBoxSize = boxSize * 0.5f;

			// The position of the light should be in the center of the box. 
			Vector3 lightPosition = lightBox.Min + halfBoxSize;
			// lightPosition.Z = lightBox.Min.Z;

			// We need the position back in world coordinates so we transform 
			// the light position by the inverse of the lights rotation
			lightPosition = Vector3.Transform(lightPosition,
											  Matrix.Invert(lightRotation));

			// Create the view matrix for the light
			var view = Matrix.CreateLookAt(lightPosition,
				lightPosition + lightDirection,
				Vector3.Up);

			// Create the projection matrix for the light
			// The projection is orthographic since we are using a directional light
			result.CameraType = CameraType.Orthographic;
			result.View = view;
			result.Width = boxSize.X;
			result.Height = boxSize.Y;
			result.NearPlane = -boxSize.Z * 16;
			result.FarPlane = boxSize.Z * 16;
		}
	}
}
