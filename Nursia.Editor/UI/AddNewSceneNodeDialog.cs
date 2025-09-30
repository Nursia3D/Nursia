using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;

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
			foreach (var pair in NodesRegistry.NodesByCategories)
			{
				List<Type> types;
				if (string.IsNullOrEmpty(filter))
				{
					types = pair.Value;
				}
				else
				{
					// Apply filter
					types = (from t in pair.Value where t.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) select t).ToList();
				}

				if (types == null || types.Count == 0)
				{
					continue;
				}

				var categoryNode = _treeType.AddSubNode(new Label
				{
					Text = pair.Key
				});

				categoryNode.IsExpanded = true;
				foreach (var nodeType in types)
				{
					var node = categoryNode.AddSubNode(new Label
					{
						Text = nodeType.Name
					});

					node.Tag = nodeType;
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