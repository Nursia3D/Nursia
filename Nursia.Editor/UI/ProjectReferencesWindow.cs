using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Nursia.Editor.Utility;
using System;

namespace Nursia.Editor.UI
{
	public partial class ProjectReferencesWindow
	{
		public ProjectReferencesWindow()
		{
			BuildUI();

			_listReferences.SelectedIndexChanged += (s, e) => UpdateEnabled();
			_buttonAdd.Click += _buttonAdd_Click;
			_buttonRemove.Click += _buttonRemove_Click;

			RefreshReferences();
			UpdateEnabled();
		}

		private void _buttonRemove_Click(object sender, EventArgs e)
		{
			var reference = (AssemblyReferenceInfo)_listReferences.SelectedItem.Tag;

			AssemblyReferenceManager.References.Remove(reference);
			_listReferences.Widgets.Remove(_listReferences.SelectedItem);
		}

		private void RefreshReferences()
		{
			_listReferences.Widgets.Clear();

			foreach (var reference in AssemblyReferenceManager.References)
			{
				var textBox = new TextBox
				{
					Text = reference.SourcePath,
					Readonly = true,
					Tag = reference
				};

				_listReferences.Widgets.Add(textBox);
			}
		}

		private void _buttonAdd_Click(object sender, EventArgs e)
		{
			try
			{
				var dialog = new FileDialog(FileDialogMode.OpenFile)
				{
					Folder = Configuration.ProjectFolder,
					Filter = "*.dll"
				};

				dialog.Closed += (s, a) =>
				{
					if (!dialog.Result)
					{
						return;
					}

					try
					{
						var assemblyPath = dialog.FilePath;
						var projectFolder = Configuration.ProjectFolder;

						// Convert to relative path
						var relativePath = PathUtils.TryToMakePathRelativeTo(assemblyPath, projectFolder);
						if (AssemblyReferenceManager.IsLoaded(relativePath))
						{
							var msgDialog = Dialog.CreateMessageBox("Info", "This assembly is already referenced.");
							msgDialog.ShowModal(Desktop);
							return;
						}

						AssemblyReferenceManager.LoadAssembly(relativePath);
						StudioGame.Instance.SaveProject();

						var confirmDialog = Dialog.CreateMessageBox("Success", $"Assembly '{System.IO.Path.GetFileName(assemblyPath)}' added to project references.");
						confirmDialog.ShowModal(Desktop);

						RefreshReferences();
					}
					catch (Exception ex)
					{
						var errorDialog = Dialog.CreateMessageBox("Error", ex.ToString());
						errorDialog.ShowModal(Desktop);
					}
				};

				dialog.ShowModal(Desktop);
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.Message);
				dialog.ShowModal(Desktop);
			}
		}

		private void UpdateEnabled()
		{
			_buttonRemove.Enabled = _listReferences.SelectedIndex != null;
		}
	}
}