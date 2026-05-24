using Nursia.Attributes;
using Nursia.SceneGraph;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nursia.Editor
{
	internal static class NodesRegistry
	{
		private static readonly List<Assembly> _assemblies = new List<Assembly>();
		private static readonly SortedDictionary<string, List<Type>> _nodesByCategories = new SortedDictionary<string, List<Type>>();
		private static readonly SortedDictionary<string, List<Type>> _nodesByAssembly = new SortedDictionary<string, List<Type>>();

		public static IReadOnlyDictionary<string, List<Type>> NodesByCategories => _nodesByCategories;
		public static IReadOnlyDictionary<string, List<Type>> NodesByAssembly => _nodesByAssembly;

		public static void AddAssembly(Assembly assembly)
		{
			if (_assemblies.Contains(assembly))
			{
				return;
			}

			var assemblyName = assembly.GetName().Name;
			var assemblyNodes = new List<Type>();

			foreach (var type in assembly.GetTypes())
			{
				// Skip abstract types and interfaces
				if (type.IsAbstract || type.IsInterface)
				{
					continue;
				}

				// Check if type is SceneNode or inherits from it
				if (!typeof(SceneNode).IsAssignableFrom(type))
				{
					continue;
				}

				// Get category from EditorInfoAttribute or use "Custom"
				var attr = type.GetCustomAttribute<EditorInfoAttribute>(false);
				var category = attr?.Category ?? "Custom";

				Nrs.LogInfo($"Adding node of type {type}");

				List<Type> types;
				if (!_nodesByCategories.TryGetValue(category, out types))
				{
					types = new List<Type>();
					_nodesByCategories[category] = types;
				}

				types.Add(type);
				assemblyNodes.Add(type);
			}

			if (assemblyNodes.Count > 0)
			{
				_nodesByAssembly[assemblyName] = assemblyNodes;
			}

			_assemblies.Add(assembly);
		}
	}
}
