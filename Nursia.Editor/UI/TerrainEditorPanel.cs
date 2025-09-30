using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Nursia.SceneGraph.Landscape;
using Nursia.Utility;
using System;

namespace Nursia.Editor.UI
{
	public partial class TerrainEditorPanel
	{
		private TerrainInstrument TerrainInstrument { get; }

		public TerrainEditorPanel(TerrainInstrument instrument)
		{
			BuildUI();

			TerrainInstrument = instrument;

			RebuildCombo();

			_comboInstrument.SelectedIndexChanged += (s, a) =>
			{
				TerrainInstrument.Type = (TerrainInstrumentType)_comboInstrument.SelectedIndex.Value;
				Update();
			};

			_comboInstrument.SelectedIndex = 0;

			_labelRadius.Text = TerrainInstrument.Radius.FormatFloat();
			_sliderRadius.Value = TerrainInstrument.Radius;

			_sliderRadius.ValueChanged += (s, a) =>
			{
				_labelRadius.Text = _sliderRadius.Value.FormatFloat();
				TerrainInstrument.Radius = _sliderRadius.Value;
			};

			_labelPower.Text = TerrainInstrument.Power.FormatFloat();
			_sliderPower.Value = TerrainInstrument.Power;
			_sliderPower.ValueChanged += (s, a) =>
			{
				_labelPower.Text = _sliderPower.Value.FormatFloat();
				TerrainInstrument.Power = _sliderPower.Value;
			};

			Update();
		}

		private void _buttonFill_Click(object sender, EventArgs e)
		{
			var instrumentType = (TerrainInstrumentType)_comboInstrument.SelectedItem.Tag;
			var dialog = Dialog.CreateMessageBox("Fill", $"Are you sure you want to fill the whole terrain with {instrumentType}");

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					return;
				}

				var c = new Color(0, 0, 0, 0);
				c.SetChannelValue(instrumentType.GetChannelIndex(), 255);
				for(var x = 0; x < TerrainInstrument.SplatWidth; ++x)
				{
					for(var y = 0; y < TerrainInstrument.SplatHeight; ++y)
					{
						TerrainInstrument.SetSplatData(x, y, c);
					}
				}

				TerrainInstrument.UpdateSplatTexture();
			};

			dialog.ShowModal(Desktop);
		}

		public void RebuildCombo()
		{
			_comboInstrument.Widgets.Clear();
			_comboInstrument.Widgets.Add(new Label
			{
				Text = "Raise",
				Tag = TerrainInstrumentType.RaiseTerrain
			});

			_comboInstrument.Widgets.Add(new Label
			{
				Text = "Lower",
				Tag = TerrainInstrumentType.LowerTerrain
			});

			var material = TerrainInstrument.Terrain.Material as TerrainMaterial;
			if (material == null)
			{
				return;
			}

			if (material.DetailMap1 != null)
			{
				AddPaint(TerrainInstrumentType.PaintTexture1, material.DetailMap1);
			}

			if (material.DetailMap2 != null)
			{
				AddPaint(TerrainInstrumentType.PaintTexture2, material.DetailMap2);
			}

			if (material.DetailMap3 != null)
			{
				AddPaint(TerrainInstrumentType.PaintTexture3, material.DetailMap3);
			}

			if (material.DetailMap4 != null)
			{
				AddPaint(TerrainInstrumentType.PaintTexture4, material.DetailMap4);
			}
		}

		private void AddPaint(TerrainInstrumentType type, Texture2D paint)
		{
			var panel = new HorizontalStackPanel
			{
				Spacing = 8,
				Tag = type
			};

			var image = new Image
			{
				Width = 32,
				Height = 32,
				Renderable = new TextureRegion(paint)
			};

			panel.Widgets.Add(image);

			var label = new Label
			{
				Text = type.ToString(),
				VerticalAlignment = VerticalAlignment.Center
			};
			panel.Widgets.Add(label);

			_comboInstrument.Widgets.Add(panel);
		}

		private void Update()
		{
			_panelAdditionalWidgets.Widgets.Clear();
			var instrumentType = (TerrainInstrumentType)_comboInstrument.SelectedItem.Tag;

			switch (instrumentType)
			{
				case TerrainInstrumentType.RaiseTerrain:
				case TerrainInstrumentType.LowerTerrain:
					{
						var panel = new HorizontalStackPanel
						{
							Spacing = 8
						};

						var label = new Label
						{
							Text = "Min Height"
						};
						panel.Widgets.Add(label);

						var numericMinHeight = new SpinButton
						{
							Value = TerrainInstrument.MinHeight,
							Width = 50
						};

						numericMinHeight.ValueChanged += (s, a) =>
						{
							TerrainInstrument.MinHeight = numericMinHeight.Value.Value;
						};
						panel.Widgets.Add(numericMinHeight);

						label = new Label
						{
							Text = "Max Height"
						};
						panel.Widgets.Add(label);

						var numericMaxHeight = new SpinButton
						{
							Value = TerrainInstrument.MaxHeight,
							Width = 50
						};

						numericMaxHeight.ValueChanged += (s, a) =>
						{
							TerrainInstrument.MaxHeight = numericMaxHeight.Value.Value;
						};
						panel.Widgets.Add(numericMaxHeight);

						_panelAdditionalWidgets.Widgets.Add(panel);
					}
					break;
				case TerrainInstrumentType.PaintTexture1:
				case TerrainInstrumentType.PaintTexture2:
				case TerrainInstrumentType.PaintTexture3:
				case TerrainInstrumentType.PaintTexture4:
					{
						var buttonFill = new Button
						{
							Content = new Label
							{
								Text = "Fill...",
								HorizontalAlignment = HorizontalAlignment.Center
							},
							Width = 100
						};

						buttonFill.Click += _buttonFill_Click;
						_panelAdditionalWidgets.Widgets.Add(buttonFill);
					}
					break;
			}
		}
	}
}