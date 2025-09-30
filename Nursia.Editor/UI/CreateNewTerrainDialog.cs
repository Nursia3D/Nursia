using Myra.Graphics2D.UI;

namespace Nursia.Editor.UI
{
	public partial class CreateNewTerrainDialog
	{
		private const int MinimumSize = 256;
		private const int MaximumSize = 4096;

		public int SplatMapSize => (int)_comboSplatMapSize.SelectedItem.Tag;

		public CreateNewTerrainDialog()
		{
			BuildUI();

			_comboSplatMapSize.Widgets.Clear();

			for (var size = MinimumSize; size <= MaximumSize; size *= 2)
			{
				var splatMapLabel = new Label
				{
					Text = $"{size}x{size}",
					Tag = size
				};

				_comboSplatMapSize.Widgets.Add(splatMapLabel);
			}

			// Default size is 1024
			_comboSplatMapSize.SelectedIndex = 2;
		}
	}
}