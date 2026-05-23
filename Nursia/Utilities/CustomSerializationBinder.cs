using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Nursia.Utilities
{
	internal class CustomSerializationBinder : ISerializationBinder
	{
		private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

		public Type BindToType(string assemblyName, string typeName)
		{
			var cacheKey = $"{assemblyName}:{typeName}";

			if (_typeCache.TryGetValue(cacheKey, out var cachedType))
			{
				return cachedType;
			}

			Type type = null;

			if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
			{
				type = Type.GetType($"{typeName}, {assemblyName}", false);
			}

			if (type == null && !string.IsNullOrEmpty(typeName))
			{
				type = Type.GetType(typeName, false);
			}

			_typeCache[cacheKey] = type;
			return type;
		}

		public void BindToName(Type type, out string assemblyName, out string typeName)
		{
			assemblyName = type.Assembly.FullName;
			typeName = type.FullName;
		}

		public static void ClearCache()
		{
			_typeCache.Clear();
		}
	}
}
