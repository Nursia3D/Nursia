using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Attributes;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Nursia.SceneGraph
{
	/// <summary>
	/// Flags that describe scene node properties.
	/// </summary>
	[Flags]
	public enum SceneFlags
	{
		/// <summary>No flags set.</summary>
		None,

		/// <summary>Node has render jobs.</summary>
		HasRenderJobs = 1 << 0
	}

	/// <summary>
	/// Base class for all nodes in a Nursia 3D scene graph.
	/// </summary>
	[EditorInfo("Base")]
	public partial class SceneNode : DrDisposable
	{
		[Flags]
		private enum DirtyFlags
		{
			None = 0,
			ActualChildren = 1 << 0,
			FullBoundingBox = 1 << 1,
			All = ActualChildren | FullBoundingBox
		}

		private static int _lastId = 0;
		private SceneNode _parent;
		private Vector3 _translation = Vector3.Zero;
		private Vector3 _rotation = Vector3.Zero;
		private Vector3 _scale = Vector3.One;
		private Matrix? _globalTransform = null, _localTransform = null, _inverseGlobalTransform = null;
		private readonly List<SceneNode> _childrenCopy = new List<SceneNode>();
		private readonly List<SceneNode> _customChildren = new List<SceneNode>();
		private readonly List<SceneNode> _actualChildren = new List<SceneNode>();
		private BoundingBox? _fullBoundingBox = null;
		private DirtyFlags _dirtyFlags = DirtyFlags.All;

		/// <summary>
		/// Gets the unique identifier for this node.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public int UniqueId { get; }

		/// <summary>
		/// Gets the flags that describe this node's properties.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public virtual SceneFlags Flags => SceneFlags.None;

		/// <summary>
		/// Gets or sets the user-defined identifier for this node.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the local position of this node relative to its parent.
		/// </summary>
		[Category("Transform")]
		public Vector3 Translation
		{
			get => _translation;

			set
			{
				if (value == _translation)
				{
					return;
				}

				_translation = value;
				InvalidateTransform();
			}
		}

		/// <summary>
		/// Gets or sets the local scale of this node.
		/// </summary>
		[Category("Transform")]
		public Vector3 Scale
		{
			get => _scale;

			set
			{
				if (value == _scale)
				{
					return;
				}

				_scale = value;
				InvalidateTransform();
			}
		}

		/// <summary>
		/// Gets or sets the local rotation of this node in degrees.
		/// </summary>
		/// <remarks>
		/// X - Pitch (rotation around X axis), Y - Yaw (rotation around Y axis), Z - Roll (rotation around Z axis).
		/// </remarks>
		[Category("Transform")]
		public Vector3 Rotation
		{
			get => _rotation;

			set
			{
				value.X = value.X.ClampDegree();
				value.Y = value.Y.ClampDegree();
				value.Z = value.Z.ClampDegree();

				if (value == _rotation)
				{
					return;
				}

				_rotation = value;
				InvalidateTransform();
			}
		}

		/// <summary>
		/// Gets or sets the local transformation matrix of this node.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix LocalTransform
		{
			get
			{
				if (_localTransform == null)
				{
					var quaternion = _rotation.ToRadians().EulerAnglesToQuaternion();
					_localTransform = SrtTransform.CreateMatrix(Translation, Scale, quaternion);
				}

				return _localTransform.Value;
			}

			set
			{
				var newLocalTransform = value;

				Vector3 scale;
				Quaternion rotation;
				Vector3 translation;
				if (!newLocalTransform.Decompose(out scale, out rotation, out translation))
				{
					return;
				}

				Translation = translation;
				var angles = rotation.ToEulerAngles();
				Rotation = angles.ToDegrees();
				Scale = scale;
			}
		}


		/// <summary>
		/// Gets or sets the global (world) transformation matrix of this node.
		/// </summary>
		/// <remarks>
		/// Setting this property is only supported for root nodes (nodes without a parent).
		/// </remarks>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix GlobalTransform
		{
			get
			{
				UpdateGlobalTransform();

				return _globalTransform.Value;
			}

			set
			{
				if (Parent != null)
				{
					throw new NotSupportedException();
				}

				LocalTransform = value;
			}
		}

		/// <summary>
		/// Gets the inverse of the global transformation matrix.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix InverseGlobalTransform
		{
			get
			{
				if (_inverseGlobalTransform == null)
				{
					_inverseGlobalTransform = Matrix.Invert(GlobalTransform);
				}

				return _inverseGlobalTransform.Value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this node and its children are visible.
		/// </summary>
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool Visible { get; set; } = true;

		/// <summary>
		/// Gets the parent node of this node.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public SceneNode Parent
		{
			get => _parent;

			internal set
			{
				if (value == _parent)
				{
					return;
				}

				_parent = value;
				InvalidateTransform();
			}
		}

		/// <summary>
		/// Gets the bounding box of this node in local space.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public virtual BoundingBox? BoundingBox => null;

		/// <summary>
		/// Gets the bounding box of this node and all its children in local space.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public BoundingBox? FullBoundingBox
		{
			get
			{
				if (_dirtyFlags.HasFlag(DirtyFlags.FullBoundingBox))
				{
					var result = BoundingBox;

					foreach (var child in ActualChildren)
					{
						result = result.Merge(child.FullBoundingBox);
					}

					_fullBoundingBox = result;

					_dirtyFlags &= ~DirtyFlags.FullBoundingBox;
				}

				return _fullBoundingBox;
			}
		}

		/// <summary>
		/// Gets the collection of child nodes attached to this node.
		/// </summary>
		[Browsable(false)]
		public ObservableCollection<SceneNode> Children { get; } = new ObservableCollection<SceneNode>();

		/// <summary>
		/// Gets the list of actual children including both user-added and custom children.
		/// </summary>
		[Browsable(false)]
		internal IReadOnlyList<SceneNode> ActualChildren
		{
			get
			{
				if (_dirtyFlags.HasFlag(DirtyFlags.ActualChildren))
				{
					foreach (var child in _customChildren)
					{
						child.Parent = null;
					}

					_actualChildren.Clear();
					_customChildren.Clear();

					var customChild = GetCustomChild();
					if (customChild != null)
					{
						customChild.Parent = this;
						_actualChildren.Add(customChild);
						_customChildren.Add(customChild);
					}

					var customChildren = GetCustomChildren();
					if (customChildren != null)
					{
						foreach (var child in customChildren)
						{
							child.Parent = this;
						}
						_actualChildren.AddRange(customChildren);
						_customChildren.AddRange(customChildren);
					}
					_actualChildren.AddRange(Children);

					_dirtyFlags &= ~DirtyFlags.ActualChildren;
				}

				return _actualChildren;
			}
		}

		/// <summary>
		/// Gets or sets user-defined data associated with this node.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public object Tag { get; set; }

		/// <summary>
		/// Gets or sets a custom action to be called during scene updates.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Action<GameTime> UpdateHandler { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SceneNode"/> class.
		/// </summary>
		public SceneNode()
		{
			UniqueId = _lastId;

			++_lastId;
			if (_lastId >= int.MaxValue)
			{
				_lastId = 0;
			}

			Children.CollectionChanged += ChildrenOnCollectionChanged;
		}

		/// <summary>
		/// Releases unmanaged and optionally managed resources used by this node.
		/// </summary>
		/// <param name="disposing">True to release both unmanaged and managed resources; false to release only unmanaged resources.</param>
		public override void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// Loads all external assets referenced by this node and its children.
		/// </summary>
		/// <param name="assetManager">The asset manager to load resources from.</param>
		public virtual void Load(AssetManager assetManager)
		{
			foreach (var child in ActualChildren)
			{
				child.Load(assetManager);
			}
		}

		private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (SceneNode n in args.NewItems)
				{
					OnChildAdded(n);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (SceneNode n in args.OldItems)
				{
					OnChildRemoved(n);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var w in _childrenCopy)
				{
					OnChildRemoved(w);
				}
			}

			OnChildrenChanged();
		}

		/// <summary>
		/// Called when the children collection has been modified.
		/// </summary>
		protected virtual void OnChildrenChanged()
		{
			_childrenCopy.Clear();
			_childrenCopy.AddRange(Children);

			InvalidateActualChildren();
		}

		/// <summary>
		/// Called when a child node has been added to this node.
		/// </summary>
		/// <param name="n">The child node that was added.</param>
		protected virtual void OnChildAdded(SceneNode n)
		{
			n.Parent = this;
		}

		/// <summary>
		/// Called when a child node has been removed from this node.
		/// </summary>
		/// <param name="n">The child node that was removed.</param>
		protected virtual void OnChildRemoved(SceneNode n)
		{
			n.Parent = null;
		}

		/// <summary>
		/// Invalidates the transformation matrices for this node and all its children.
		/// </summary>
		public virtual void InvalidateTransform()
		{
			_localTransform = null;
			_globalTransform = null;
			_inverseGlobalTransform = null;

			foreach (var child in ActualChildren)
			{
				child.InvalidateTransform();
			}
		}

		/// <summary>
		/// Traverses the scene graph starting from this node, calling the handler for each node.
		/// </summary>
		/// <param name="handler">The action to invoke for each node in the tree.</param>
		public void Traverse(Action<SceneNode> handler)
		{
			handler(this);

			foreach (var child in ActualChildren)
			{
				child.Traverse(handler);
			}
		}

		/// <summary>
		/// Traverses the scene graph starting from this node, calling the handler for each node with a parameter.
		/// </summary>
		/// <typeparam name="T">The type of the parameter to pass to the handler.</typeparam>
		/// <param name="handler">The action to invoke for each node in the tree.</param>
		/// <param name="param">A parameter to pass to the handler.</param>
		public void Traverse<T>(Action<SceneNode, T> handler, T param)
		{
			handler(this, param);

			foreach (var child in ActualChildren)
			{
				child.Traverse(handler, param);
			}
		}

		/// <summary>
		/// Traverses the scene graph starting from this node, calling the handler for each node with two parameters.
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter.</typeparam>
		/// <typeparam name="T2">The type of the second parameter.</typeparam>
		/// <param name="handler">The action to invoke for each node in the tree.</param>
		/// <param name="param1">The first parameter to pass to the handler.</param>
		/// <param name="param2">The second parameter to pass to the handler.</param>
		public void Traverse<T1, T2>(Action<SceneNode, T1, T2> handler, T1 param1, T2 param2)
		{
			handler(this, param1, param2);

			foreach (var child in ActualChildren)
			{
				child.Traverse(handler, param1, param2);
			}
		}

		private void InternalQuery(Func<SceneNode, bool> predicate, List<SceneNode> result)
		{
			if (predicate(this))
			{
				result.Add(this);
			}

			foreach (var child in ActualChildren)
			{
				child.InternalQuery(predicate, result);
			}
		}

		/// <summary>
		/// Queries the scene graph for all nodes matching the specified predicate.
		/// </summary>
		/// <param name="predicate">The condition to match against each node.</param>
		/// <returns>A list of all nodes that satisfy the predicate.</returns>
		public List<SceneNode> Query(Func<SceneNode, bool> predicate)
		{
			var result = new List<SceneNode>();
			InternalQuery(predicate, result);

			return result;
		}

		/// <summary>
		/// Queries the scene graph for the first node matching the specified predicate.
		/// </summary>
		/// <param name="predicate">The condition to match against each node.</param>
		/// <returns>The first node that satisfies the predicate, or null if no match is found.</returns>
		public SceneNode QueryFirst(Func<SceneNode, bool> predicate)
		{
			if (predicate(this))
			{
				return this;
			}

			foreach (var child in ActualChildren)
			{
				var result = child.QueryFirst(predicate);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// Queries the scene graph for the first node of a specific type.
		/// </summary>
		/// <typeparam name="T">The type of node to find.</typeparam>
		/// <returns>The first node of the specified type, or null if no match is found.</returns>
		public T QueryFirstByType<T>() where T : SceneNode => (T)QueryFirst(s => s is T);

		/// <summary>
		/// Queries the scene graph for the first node with the specified identifier.
		/// </summary>
		/// <param name="id">The identifier to search for.</param>
		/// <returns>The first node with the specified identifier, or null if no match is found.</returns>
		public SceneNode QueryFirstById(string id) => QueryFirst(s => s.Id == id);


		private void InternalQueryByType<T>(List<T> result) where T : SceneNode
		{
			var asT = this as T;
			if (asT != null)
			{
				result.Add(asT);
			}

			foreach (var child in ActualChildren)
			{
				child.InternalQueryByType(result);
			}
		}

		/// <summary>
		/// Queries the scene graph for all nodes of a specific type.
		/// </summary>
		/// <typeparam name="T">The type of nodes to find.</typeparam>
		/// <returns>A list of all nodes of the specified type.</returns>
		public List<T> QueryByType<T>() where T : SceneNode
		{
			var result = new List<T>();

			InternalQueryByType(result);

			return result;
		}

		/// <summary>
		/// Removes this node from its parent's children collection.
		/// </summary>
		public void RemoveFromParent()
		{
			if (Parent == null)
			{
				return;
			}

			Parent.Children.Remove(this);
		}

		/// <summary>
		/// Creates a deep copy of this node and all its children.
		/// </summary>
		/// <returns>A new node that is a copy of this node.</returns>
		public SceneNode Clone()
		{
			var result = CreateInstanceCore();
			if (result.GetType() != GetType())
			{
				throw new Exception($"Node of type {GetType()} didnt implement cloning.");
			}

			result.CopyFrom(this);

			return result;
		}

		/// <summary>
		/// Updates the global transformation matrix for this node based on its parent's global transform.
		/// </summary>
		protected void UpdateGlobalTransform()
		{
			if (_globalTransform != null)
			{
				return;
			}

			if (Parent != null)
			{
				_globalTransform = LocalTransform * Parent.GlobalTransform;
			}
			else
			{
				_globalTransform = LocalTransform;
			}

			OnGlobalTransformUpdated();
		}

		/// <summary>
		/// Called after the global transformation matrix has been updated.
		/// </summary>
		protected virtual void OnGlobalTransformUpdated()
		{
		}

		/// <summary>
		/// Creates a new instance of this node type. Derived classes should override this method to create instances of their own type.
		/// </summary>
		/// <returns>A new instance of this node.</returns>
		protected virtual SceneNode CreateInstanceCore()
		{
			return new SceneNode();
		}

		/// <summary>
		/// Copies all properties from another node to this node.
		/// </summary>
		/// <param name="node">The source node to copy from.</param>
		protected virtual void CopyFrom(SceneNode node)
		{
			Id = node.Id;
			Translation = node.Translation;
			Scale = node.Scale;
			Rotation = node.Rotation;
			Visible = node.Visible;
			Tag = node.Tag;

			Children.Clear();
			foreach (var child in node.Children)
			{
				Children.Add(child.Clone());
			}
		}

		/// <summary>
		/// Adds render jobs for this node to the specified batch.
		/// </summary>
		/// <param name="camera">The camera used for rendering.</param>
		/// <param name="batch">The render jobs batch to add jobs to.</param>
		public virtual void AddRenderJobs(Camera camera, IRenderJobsBatch batch)
		{
		}

		/// <summary>
		/// Invalidates the full bounding box for this node and all parent nodes.
		/// </summary>
		protected void InvalidateFullBoundingBox()
		{
			_dirtyFlags |= DirtyFlags.FullBoundingBox;
			Parent?.InvalidateFullBoundingBox();
		}

		/// <summary>
		/// Invalidates the cached list of actual children and the bounding box.
		/// </summary>
		protected void InvalidateActualChildren()
		{
			_dirtyFlags |= DirtyFlags.ActualChildren;
			InvalidateFullBoundingBox();
		}

		/// <summary>
		/// Should be overriden by nodes that may have one custom child
		/// </summary>
		/// <returns></returns>
		protected virtual SceneNode GetCustomChild()
		{
			return null;
		}

		/// <summary>
		/// Should be overriden by nodes that may have multiple custom children
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<SceneNode> GetCustomChildren()
		{
			return null;
		}
	}
}
