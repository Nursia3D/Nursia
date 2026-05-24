using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nursia.Editor.UI
{
	public partial class AddNewSceneNodeDialog
	{
		private readonly TreeView _treeType;

		public string Name => _textName.Text;
		public Type Type => (Type)_treeType.SelectedNode.Tag;

		public bool RequireFilename
		{
			get => _panelFilename.Visible;

			set => _panelFilename.Visible = value;
		}

		public AddNewSceneNodeDialog()
		{
			BuildUI();

			_treeType = new TreeView();
			RebuildTree();

			_panelNodes.Content = _treeType;

			_textName.TextChanged += (s, a) => UpdateEnabled();
			_treeType.SelectionChanged += (s, a) => UpdateEnabled();
			_textFilter.TextChanged += (s, a) => RebuildTree();
		}

		private void RebuildTree()
		{
			_treeType.RemoveAllSubNodes();
			var filter = _textFilter.Text;

			// Add custom nodes from referenced assemblies first
			var customAssemblies = NodesRegistry.NodesByAssembly;
			if (customAssemblies != null && customAssemblies.Count > 0)
			{
				foreach (var assemblyPair in customAssemblies)
				{
					List<Type> types;
					if (string.IsNullOrEmpty(filter))
					{
						types = assemblyPair.Value;
					}
					else
					{
						// Apply filter
						types = (from t in assemblyPair.Value where t.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) select t).ToList();
					}

					if (types == null || types.Count == 0)
					{
						continue;
					}

					// Create assembly node
					var assemblyNode = _treeType.AddSubNode(new Label
					{
						Text = $"[{assemblyPair.Key}]"
					});
					assemblyNode.IsExpanded = true;

					// Group types by category within this assembly
					var typesByCategory = new Dictionary<string, List<Type>>();
					foreach (var nodeType in types)
					{
						var attr = nodeType.GetCustomAttribute<Nursia.Attributes.EditorInfoAttribute>(false);
						var category = attr?.Category ?? "Other";

						if (!typesByCategory.ContainsKey(category))
						{
							typesByCategory[category] = new List<Type>();
						}
						typesByCategory[category].Add(nodeType);
					}

					// Add categories and types under assembly node
					foreach (var categoryPair in typesByCategory.OrderBy(p => p.Key))
					{
						var categoryNode = assemblyNode.AddSubNode(new Label
						{
							Text = categoryPair.Key
						});
						categoryNode.IsExpanded = true;

						foreach (var nodeType in categoryPair.Value)
						{
							var node = categoryNode.AddSubNode(new Label
							{
								Text = nodeType.Name
							});
							node.Tag = nodeType;
						}
					}
				}
			}

			UpdateEnabled();
		}

		public void UpdateEnabled()
		{
			var filenameCheck = RequireFilename ? !string.IsNullOrEmpty(_textName.Text) : true;

			var node = _treeType.SelectedNode;
			ButtonOk.Enabled = filenameCheck && node != null && node.Tag != null;
		}
	}
}