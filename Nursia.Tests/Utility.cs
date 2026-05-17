using AssetManagementBase;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Nursia.Tests
{
	internal static class Utility
	{
		/// <summary>
		/// The value for which all absolute numbers smaller than are considered equal to zero.
		/// </summary>
		public const float ZeroTolerance = 1e-6f;

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static AssetManager CreateAssetManager()
		{
			return AssetManager.CreateFileAssetManager(Path.Combine(ExecutingAssemblyDirectory, "Assets"));
		}

		public static void AssertAreEqual(float expected, float actual, float epsilon = ZeroTolerance)
		{
			var precision = (int)Math.Max(0, Math.Ceiling(-Math.Log10(epsilon)));
			Assert.Equal(expected, actual, precision);
		}

		public static void AssertAreEqual(Vector2 a, Vector2 b, float epsilon = ZeroTolerance)
		{
			var precision = (int)Math.Max(0, Math.Ceiling(-Math.Log10(epsilon)));
			Assert.Equal(a.X, b.X, precision);
			Assert.Equal(a.Y, b.Y, precision);
		}

		public static void AssertAreEqual(Vector3 a, Vector3 b, float epsilon = ZeroTolerance)
		{
			var precision = (int)Math.Max(0, Math.Ceiling(-Math.Log10(epsilon)));
			Assert.Equal(a.X, b.X, precision);
			Assert.Equal(a.Y, b.Y, precision);
			Assert.Equal(a.Z, b.Z, precision);
		}

		public static void AssertAreEqual(Vector4 a, Vector4 b, float epsilon = ZeroTolerance)
		{
			var precision = (int)Math.Max(0, Math.Ceiling(-Math.Log10(epsilon)));
			Assert.Equal(a.X, b.X, precision);
			Assert.Equal(a.Y, b.Y, precision);
			Assert.Equal(a.Z, b.Z, precision);
			Assert.Equal(a.W, b.W, precision);
		}

		public static void AssertAreEqual(BoundingBox a, BoundingBox b, float epsilon = ZeroTolerance)
		{
			AssertAreEqual(a.Min, b.Min, epsilon);
			AssertAreEqual(a.Max, b.Max, epsilon);
		}
	}
}
