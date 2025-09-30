using AssetManagementBase;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;

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
			Assert.AreEqual(expected, actual, epsilon);
		}

		public static void AssertAreEqual(Vector2 a, Vector2 b, float epsilon = ZeroTolerance)
		{
			Assert.AreEqual(a.X, b.X, epsilon);
			Assert.AreEqual(a.Y, b.Y, epsilon);
		}

		public static void AssertAreEqual(Vector3 a, Vector3 b, float epsilon = ZeroTolerance)
		{
			Assert.AreEqual(a.X, b.X, epsilon);
			Assert.AreEqual(a.Y, b.Y, epsilon);
			Assert.AreEqual(a.Z, b.Z, epsilon);
		}

		public static void AssertAreEqual(Vector4 a, Vector4 b, float epsilon = ZeroTolerance)
		{
			Assert.AreEqual(a.X, b.X, epsilon);
			Assert.AreEqual(a.Y, b.Y, epsilon);
			Assert.AreEqual(a.Z, b.Z, epsilon);
			Assert.AreEqual(a.W, b.W, epsilon);
		}

		public static void AssertAreEqual(BoundingBox a, BoundingBox b, float epsilon = ZeroTolerance)
		{
			AssertAreEqual(a.Min, b.Min, epsilon);
			AssertAreEqual(a.Max, b.Max, epsilon);
		}
	}
}
