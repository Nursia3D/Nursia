using System;
using System.Collections.Concurrent;

namespace Nursia.Utilities
{
	internal class ObjectPool<T>
	{
		private readonly ConcurrentBag<T> _objects = new ConcurrentBag<T>();
		private readonly Func<T> _creator;
		public ObjectPool(Func<T> creator)
		{
			_creator = creator ?? throw new ArgumentNullException(nameof(creator));
		}

		public T Get() => _objects.TryTake(out T item) ? item : _creator();

		public void Recycle(T item) => _objects.Add(item);
	}
}
