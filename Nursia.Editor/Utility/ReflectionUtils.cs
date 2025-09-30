using System;
using System.Reflection;

namespace Nursia.Editor.Utility
{
	internal static class ReflectionUtils
	{
		public static PropertyInfo GetPropertyWithAttribute<T>(this Type type) where T : Attribute
		{
			var props = type.GetProperties();

			foreach (var p in props)
			{
				if (p.GetCustomAttribute<T>() != null)
				{
					return p;
				}
			}

			return null;
		}
	}
}
