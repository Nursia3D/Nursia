using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Nursia.Editor
{
	public class AssemblyReferenceInfo
	{
		public string SourcePath { get; }
		public string FullPath { get; }
		public Assembly Assembly { get; }

		public AssemblyReferenceInfo(string path)
		{
			SourcePath = path;
			FullPath = ResolveAssemblyPath(path);
			Assembly = Assembly.LoadFrom(FullPath);
		}

		public static string ResolveAssemblyPath(string path)
		{
			// If path is absolute, use it as-is
			if (Path.IsPathRooted(path))
			{
				return path;
			}

			return Path.Combine(Configuration.ProjectFolder, path);
		}
	}

	public static class AssemblyReferenceManager
	{
		public static List<AssemblyReferenceInfo> References { get; } = new List<AssemblyReferenceInfo>();

		public static bool IsLoaded(string path)
		{
			var fullPath = AssemblyReferenceInfo.ResolveAssemblyPath(path);
			return (from r in References where r.FullPath == fullPath select r).Any();
		}

		public static void LoadAssembly(string path)
		{
			if (IsLoaded(path))
			{
				return;
			}

			var newReference = new AssemblyReferenceInfo(path);
			References.Add(newReference);
			NodesRegistry.AddAssembly(newReference.Assembly);
		}

		public static void LoadAssemblies(params string[] paths)
		{
			if (paths == null)
			{
				return;
			}

			foreach (var path in paths)
			{
				try
				{
					LoadAssembly(path);
				}
				catch (Exception ex)
				{
					Nrs.LogError($"Failed to load assembly '{path}': {ex.Message}");
				}
			}
		}

		public static void ClearLoadedAssemblies()
		{
			References.Clear();
		}

		public static Type ResolveType(string typeName)
		{
			foreach (var reference in References)
			{
				var type = reference.Assembly.GetType(typeName);
				if (type != null)
				{
					return type;
				}
			}

			return null;
		}
	}
}
