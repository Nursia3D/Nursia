using System.Collections.Generic;

namespace Nursia.Utilities
{
	internal static class Utils
	{
		/// <summary>
		/// Standard path separator character (forward slash) used internally.
		/// </summary>
		public const char SeparatorSymbol = '/';

		/// <summary>
		/// Standard path separator string (forward slash) used internally.
		/// </summary>
		public const string SeparatorString = "/";

		public static void Swap<T>(ref T a, ref T b)
		{
			var temp = a;
			a = b;
			b = temp;
		}

		public static string NormalizeFilePath(this string path)
		{
			path = path.Replace(@"\", SeparatorSymbol.ToString());

			if (!path.Contains(".."))
			{
				return path;
			}

			var parts = path.Split(SeparatorSymbol);
			var partsStack = new List<string>();
			for (var i = 0; i < parts.Length; i++)
			{
				if (parts[i] == ".." && partsStack.Count > 0 && partsStack[partsStack.Count - 1] != ".." && partsStack[partsStack.Count - 1] != ".")
				{
					// Go up one level by removing the last path component
					partsStack.RemoveAt(partsStack.Count - 1);
				}
				else if (!string.IsNullOrEmpty(parts[i]))
				{
					// Add non-empty, non-current-dir components
					partsStack.Add(parts[i]);
				}
			}

			// Preserve rooted status
			if (path.StartsWith(SeparatorString))
			{
				path = SeparatorSymbol + string.Join(SeparatorString, partsStack);
			}
			else
			{
				path = string.Join(SeparatorString, partsStack);
			}

			return path;
		}
	}
}
