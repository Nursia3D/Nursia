using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nursia.Rendering
{
	internal class GraphicsResourcePool<T> where T : GraphicsResource
	{
		private class GraphicsResourceStorageInfo
		{
			public string Name { get; }
			public int Count { get; set; }
			public bool Stored { get; set; }

			public GraphicsResourceStorageInfo(string name)
			{
				Name = name;
			}
		}

		protected readonly List<T> _storage = new List<T>();
		private readonly List<int> _toDelete = new List<int>();

		protected T GetFromStorage(int index)
		{
			var result = _storage[index];

			// Erase its stored flag and remove from the storage
			var info = (GraphicsResourceStorageInfo)result.Tag;
			info.Stored = false;
			_storage.RemoveAt(index);

			// Return
			return result;
		}

		protected void OnTargetCreated(string name, T item)
		{
			var info = new GraphicsResourceStorageInfo(name);
			item.Tag = info;

			Debug.WriteLine($"Created {typeof(T).Name} '{info.Name}'");
		}

		public void Recycle(T target)
		{
			var info = (GraphicsResourceStorageInfo)target.Tag;
			if (info.Stored)
			{
				// Already in storage
				return;
			}

			// Start the count and put to the storage
			info.Count = 0;
			info.Stored = true;
			_storage.Add(target);
		}

		public void Update()
		{
			_toDelete.Clear();
			for (var i = 0; i < _storage.Count; ++i)
			{
				var target = _storage[i];

				// Update the counter
				var info = (GraphicsResourceStorageInfo)target.Tag;
				info.Count++;

				if (info.Count >= 10)
				{
					// Schedule for deletion
					_toDelete.Add(i);
				}
			}

			// Deletion
			for (var i = _toDelete.Count - 1; i >= 0; --i)
			{
				var target = _storage[_toDelete[i]];
				var info = (GraphicsResourceStorageInfo)target.Tag;
				target.Dispose();
				_storage.RemoveAt(_toDelete[i]);

				Debug.WriteLine($"Deleted {typeof(T).Name} '{info.Name}'");
			}
		}
	}
}
