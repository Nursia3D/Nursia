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
	/// Base 3D Scene Node
	/// </summary>
	[EditorInfo("Base")]
	public class SceneNode : DrDisposable
	{
		private SceneNode _parent;
		private Vector3 _translation = Vector3.Zero;
		private Vector3 _rotation = Vector3.Zero;
		private Vector3 _scale = Vector3.One;
		private Matrix? _globalTransform = null, _localTransform = null, _inverseGlobalTransform = null;
		private readonly List<SceneNode> _childrenCopy = new List<SceneNode>();

		public string Id { get; set; }

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
		/// X - Pitch, Y - yaw, Z - roll
		/// </summary>
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

		[Browsable(false)]
		[JsonIgnore]
		public Matrix LocalTransform
		{
			get
			{
				if (_localTransform == null)
				{
					var quaternion = Quaternion.CreateFromYawPitchRoll(
											MathHelper.ToRadians(_rotation.Y),
											MathHelper.ToRadians(_rotation.X),
											MathHelper.ToRadians(_rotation.Z));
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

		[DefaultValue(true)]
		[Category("Behavior")]

		public bool Visible { get; set; } = true;

		[Browsable(false)]
		[JsonIgnore]
		public SceneNode Parent
		{
			get => _parent;

			set
			{
				if (value == _parent)
				{
					return;
				}

				_parent = value;
				InvalidateTransform();
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public virtual BoundingBox? BoundingBox => null;

		[Browsable(false)]
		public ObservableCollection<SceneNode> Children { get; } = new ObservableCollection<SceneNode>();

		[Browsable(false)]
		protected internal virtual IReadOnlyCollection<SceneNode> ActualChildren => Children;

		[Browsable(false)]
		[JsonIgnore]
		public object Tag { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public Action PreRender { get; set; }

		public SceneNode()
		{
			Children.CollectionChanged += ChildrenOnCollectionChanged;
		}

		public override void Dispose(bool disposing)
		{
		}

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

		protected virtual void OnChildrenChanged()
		{
			_childrenCopy.Clear();
			_childrenCopy.AddRange(Children);
		}

		protected virtual void OnChildAdded(SceneNode n)
		{
			n.Parent = this;
		}

		protected virtual void OnChildRemoved(SceneNode n)
		{
			n.Parent = null;
		}

		protected internal virtual void Render(IRenderBatch batch)
		{
			PreRender?.Invoke();
		}

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

		public List<SceneNode> Query(Func<SceneNode, bool> predicate)
		{
			var result = new List<SceneNode>();
			InternalQuery(predicate, result);

			return result;
		}

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

		public T QueryFirstByType<T>() where T : SceneNode => (T)QueryFirst(s => s is T);
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

		public List<T> QueryByType<T>() where T : SceneNode
		{
			var result = new List<T>();

			InternalQueryByType(result);

			return result;
		}

		private static void IterateInternal(SceneNode node, Action<SceneNode> action)
		{
			action(node);

			foreach (var child in node.ActualChildren)
			{
				IterateInternal(child, action);
			}
		}

		public void Iterate(Action<SceneNode> action)
		{
			IterateInternal(this, action);
		}

		public void RemoveFromParent()
		{
			if (Parent == null)
			{
				return;
			}

			Parent.Children.Remove(this);
		}

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

		protected virtual void OnGlobalTransformUpdated()
		{
		}

		protected virtual SceneNode CreateInstanceCore()
		{
			return new SceneNode();
		}

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
	}
}
