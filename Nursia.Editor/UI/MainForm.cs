using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Myra.Graphics2D.UI.Properties;
using Nursia.Attributes;
using Nursia.Editor.Utility;
using Nursia.Materials;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Landscape;
using StbImageWriteSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nursia.Editor.UI
{
	public partial class MainForm
	{
		private enum TouchDownType
		{
			None,
			Explorer,
			FileSystem
		}

		private class CustomLoadWidgetInfo
		{
			public Type BaseType;
			public string Filter;
			public Func<AssetManager, string, object> Loader;

			public CustomLoadWidgetInfo(Type baseType, string filter, Func<AssetManager, string, object> loader)
			{
				BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
				Filter = filter;
				Loader = loader ?? throw new ArgumentNullException(nameof(loader));
			}
		}

		private class CustomSelectWidgetInfo
		{
			public Type BaseType;
			public GraphicsResource[] Variants;

			public CustomSelectWidgetInfo(Type baseType, GraphicsResource[] variants)
			{
				BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
				Variants = variants ?? throw new ArgumentNullException(nameof(variants));
			}
		}

		private static CustomLoadWidgetInfo[] _customLoadWidgetsInfo = new CustomLoadWidgetInfo[]
		{
			new CustomLoadWidgetInfo(typeof(Texture), "*.dds|*.png|*.jpg|*.gif|*.bmp|*.tga", (m, n) => m.LoadTexture(Nrs.GraphicsDevice, n)),
			new CustomLoadWidgetInfo(typeof(SpriteFont), "*.fnt", (m, n) => m.LoadSpriteFont(Nrs.GraphicsDevice, n)),
			new CustomLoadWidgetInfo(typeof(DrModel), "*.gltf|*.glb", (m, n) => m.LoadModel(Nrs.GraphicsDevice, n, ModelLoadFlags.EnsureUVs)),
			new CustomLoadWidgetInfo(typeof(SceneNode), "*.scene", (m, n) => m.LoadSceneNode(n)),
			new CustomLoadWidgetInfo(typeof(HeightField), "*.hf", (m, n) => LoadHeightField(m, n)),
		};

		private static CustomSelectWidgetInfo[] _customSelectWidgetsInfo = new CustomSelectWidgetInfo[]
		{
			new CustomSelectWidgetInfo(typeof(BlendState), new GraphicsResource[] { BlendState.AlphaBlend, BlendState.NonPremultiplied, BlendState.Opaque, BlendState.Additive }),
			new CustomSelectWidgetInfo(typeof(DepthStencilState), new GraphicsResource[] { DepthStencilState.Default, DepthStencilState.DepthRead, DepthStencilState.None }),
			new CustomSelectWidgetInfo(typeof(RasterizerState), new GraphicsResource[] { RasterizerState.CullCounterClockwise, RasterizerState.CullClockwise, RasterizerState.CullNone }),
		};

		private const string ButtonsPanelId = "_buttonsPanel";
		private const string ButtonCameraViewId = "_buttonCameraView";
		private TouchDownType _explorerTouchDown = TouchDownType.None;
		private readonly TreeView _treeFileExplorer, _treeFileSolution;
		private readonly ScrollViewer _scrollViewerModel;
		private readonly TreeView _treeModel;

		public object SelectedObject
		{
			get => _propertyGrid.Object;
			set => _propertyGrid.Object = value;
		}

		public SceneWidget CurrentSceneWidget
		{
			get
			{
				if (_tabControlScenes.SelectedIndex == null)
				{
					return null;
				}

				return _tabControlScenes.SelectedItem.Content.FindChild<SceneWidget>();
			}
		}

		public TerrainInstrument TerrainInstrument { get; } = new TerrainInstrument();

		public IEnumerable<string> OpenFiles
		{
			get
			{
				foreach (var tab in _tabControlScenes.Items)
				{
					var tabInfo = (TabInfo)tab.Tag;

					yield return tabInfo.FilePath;
				}

				yield break;
			}
		}

		public Camera CurrentCamera
		{
			get
			{
				var camera = CurrentSceneWidget.Scene.Camera;

				var asCamera = SelectedObject as Camera;
				if (asCamera != null)
				{
					var buttonCameraView = _tabControlScenes.SelectedItem.Content.FindChildById<ToggleButton>(ButtonCameraViewId);
					if (buttonCameraView != null && buttonCameraView.IsToggled)
					{
						camera = asCamera;
					}
				}

				return camera;
			}
		}

		public TerrainEditorPanel TerrainEditorPanel
		{
			get
			{
				var tab = _tabControlScenes.SelectedItem;
				if (tab == null)
				{
					return null;
				}

				var buttonsGrid = _tabControlScenes.SelectedItem.Content.FindChildById<StackPanel>(ButtonsPanelId);
				if (buttonsGrid == null || buttonsGrid.Widgets.Count == 0)
				{
					return null;
				}

				return buttonsGrid.Widgets[0] as TerrainEditorPanel;
			}
		}

		public bool ShowCameraPosition
		{
			get => _buttonCameraPosition.IsToggled;

			set => _buttonCameraPosition.IsToggled = value;
		}

		public event EventHandler SelectedObjectChanged
		{
			add
			{
				_treeFileExplorer.SelectionChanged += value;
			}

			remove
			{
				_treeFileExplorer.SelectionChanged -= value;
			}
		}

		public MainForm()
		{
			BuildUI();

			_treeFileSolution = new TreeView
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ClipToBounds = true
			};
			_panelSolution.Content = _treeFileSolution;

			_treeFileExplorer = new TreeView
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ClipToBounds = true
			};
			_panelSceneExplorer.Content = _treeFileExplorer;

			_treeModel = new TreeView
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ClipToBounds = true
			};

			_scrollViewerModel = new ScrollViewer
			{
				Content = _treeModel
			};

			StackPanel.SetProportionType(_scrollViewerModel, ProportionType.Part);
			StackPanel.SetProportionValue(_scrollViewerModel, 1.0f);

			_topSplitPane.SetSplitterPosition(0, 0.2f);
			_topSplitPane.SetSplitterPosition(1, 0.6f);

			_menuItemSaveCurrentItem.Selected += (s, a) => SaveCurrentItem();
			_menuItemSaveEverything.Selected += _menuItemSaveEverything_Selected;
			_menuItemReloadCurrentItem.Selected += _menuItemReloadCurrentItem_Selected;
			_menuItemProjectReferences.Selected += _menuItemProjectReferences_Selected;

			_menuItemGraphicsSettings.Selected += _menuItemGraphicsSettings_Selected;
			_menuItemRenderEnvironment.Selected += _menuItemRenderEnvironment_Selected;
			_menuItemDebugSettings.Selected += _menuItemDebugSettings_Selected;

			_treeFileExplorer.TouchDown += (s, e) =>
			{
				var mouseState = Mouse.GetState();
				if (mouseState.RightButton == ButtonState.Pressed)
				{
					_explorerTouchDown = TouchDownType.Explorer;
				}
			};
			_treeFileExplorer.TouchUp += _treeFileExplorer_TouchUp;
			_treeFileExplorer.SelectionChanged += _treeFileExplorer_SelectionChanged;

			_treeFileSolution.TouchDoubleClick += (s, a) => OpenCurrentSolutionItem();
			_treeFileSolution.TouchDown += (s, e) =>
			{
				var mouseState = Mouse.GetState();
				if (mouseState.RightButton == ButtonState.Pressed)
				{
					_explorerTouchDown = TouchDownType.FileSystem;
				}
			};
			_treeFileSolution.TouchUp += _treeFileSolution_TouchUp;

			_treeModel.SelectionChanged += _treeModel_SelectionChanged;

			_propertyGrid.PropertyChanged += (s, a) =>
			{
				InvalidateCurrentItem();

				switch (a.Data)
				{
					case "Id":
						UpdateTreeNodeId(_treeFileExplorer.SelectedNode);
						break;

					case "PrimitiveMeshType":
						_propertyGrid.Rebuild();
						break;
				}
			};

			_propertyGrid.CustomWidgetProvider = CreateCustomEditor;

			_tabControlScenes.Items.Clear();
			_tabControlScenes.SelectedIndexChanged += (s, a) => RefreshExplorer(null);
			_tabControlScenes.ItemsCollectionChanged += (s, a) => UpdateStackPanelEditor();

			_buttonGrid.IsToggled = NursiaEditorOptions.ShowGrid;

			_buttonGrid.IsToggledChanged += (s, a) => UpdateEditorOptions();
			_buttonCameraPosition.IsToggledChanged += (s, a) => _labelCameraPosition.Visible = _buttonCameraPosition.IsToggled;

			UpdateStackPanelEditor();
		}

		private void _menuItemProjectReferences_Selected(object sender, EventArgs e)
		{
			var window = new ProjectReferencesWindow();
			window.ShowModal(Desktop);
		}

		private void _menuItemDebugSettings_Selected(object sender, EventArgs e)
		{
			var window = new Window
			{
				Title = "Debug Settings"
			};

			var propertyGrid = new PropertyGrid
			{
				CustomWidgetProvider = CreateCustomEditor,
				Object = Nrs.DebugSettings
			};

			window.Content = propertyGrid;

			window.Show(Desktop);
		}

		private void _menuItemRenderEnvironment_Selected(object sender, EventArgs e)
		{
			var window = new Window
			{
				Title = "Render Environment"
			};

			var propertyGrid = new PropertyGrid
			{
				CustomWidgetProvider = CreateCustomEditor,
				Object = CurrentSceneWidget.Scene.RenderEnvironment
			};

			window.Content = propertyGrid;

			window.Show(Desktop);
		}

		private void _menuItemGraphicsSettings_Selected(object sender, EventArgs e)
		{
			var window = new Window
			{
				Title = "Graphics Settings"
			};

			var propertyGrid = new PropertyGrid
			{
				CustomWidgetProvider = CreateCustomEditor,
				Object = Nrs.GraphicsSettings
			};

			window.Content = propertyGrid;

			window.Show(Desktop);
		}

		private void RebuildProperties()
		{
			_panelPropertiesButtons.Widgets.Clear();
			_propertyGrid.Rebuild();
			CheckHasDefaultMaterial();
			CheckModelSelected();
			InvalidateCurrentItem();
		}

		private Widget CreateLoadCustomEditor(Record record, object obj)
		{
			CustomLoadWidgetInfo cwi = null;
			foreach (var c in _customLoadWidgetsInfo)
			{
				if (c.BaseType.IsAssignableFrom(record.Type))
				{
					cwi = c;
					break;
				}
			}

			if (cwi == null)
			{
				return null;
			}

			var pathProperty = obj.GetType().GetProperty(record.Name + "Path");
			if (pathProperty == null)
			{
				throw new Exception($"Can't find property {record.Name + "Path"}");
			}

			if (pathProperty.PropertyType != typeof(string))
			{
				throw new Exception($"property {record.Name + "Path"} isn't of type string");
			}

			var propertyType = record.Type;

			var result = new HorizontalStackPanel
			{
				Spacing = 8
			};

			var path = (string)pathProperty.GetValue(obj);
			var pathEditor = new TextBox
			{
				Readonly = true,
				Text = path
			};

			StackPanel.SetProportionType(pathEditor, ProportionType.Fill);
			result.Widgets.Add(pathEditor);

			var button = new Button
			{
				Tag = obj,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Content = new Label
				{
					Text = "Change...",
					HorizontalAlignment = HorizontalAlignment.Center,
				}
			};
			Grid.SetColumn(button, 1);

			button.Click += (sender, args) =>
			{
				try
				{
					var dialog = new FileDialog(FileDialogMode.OpenFile)
					{
						Folder = Configuration.LastAssetsFolder,
						Filter = cwi.Filter
					};

					dialog.Closed += (s, a) =>
					{
						if (!dialog.Result)
						{
							// "Cancel" or Escape
							return;
						}

						// "Ok" or Enter
						try
						{
							path = dialog.FilePath;
							var folder = Path.GetDirectoryName(path);
							var assetManager = AssetManager.CreateFileAssetManager(folder);

							var result = cwi.Loader(assetManager, path);

							record.SetValue(obj, result);

							path = PathUtils.TryToMakePathRelativeTo(dialog.FilePath, Configuration.ProjectFolder);
							pathProperty.SetValue(obj, path);

							RebuildProperties();

							switch (record.Name)
							{
								case "DetailMap1":
								case "DetailMap2":
								case "DetailMap3":
								case "DetailMap4":
									var panel = TerrainEditorPanel;
									panel?.RebuildCombo();
									break;
							}

							Configuration.LastAssetsFolder = folder;
						}
						catch (Exception ex)
						{
							var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
							dialog.ShowModal(Desktop);
						}
					};

					dialog.ShowModal(Desktop);
				}
				catch (Exception ex)
				{
					var dialog = Dialog.CreateMessageBox("Error", ex.Message);
					dialog.ShowModal(Desktop);
				}
			};

			result.Widgets.Add(button);

			return result;
		}

		private Widget CreateCustomSelectEditor(Record record, object obj)
		{
			CustomSelectWidgetInfo cwi = null;
			foreach (var c in _customSelectWidgetsInfo)
			{
				if (c.BaseType.IsAssignableFrom(record.Type))
				{
					cwi = c;
					break;
				}
			}

			if (cwi == null)
			{
				return null;
			}

			var combo = new ComboView();

			// Null value
			var idx = 0;
			combo.Widgets.Add(new Label
			{
				Text = "(null)"
			});

			for (var i = 0; i < cwi.Variants.Length; ++i)
			{
				var v = cwi.Variants[i];

				var parts = v.Name.Split(".");
				combo.Widgets.Add(new Label
				{
					Text = parts[parts.Length - 1],
					Tag = v
				});

				if (record.GetValue(obj) == v)
				{
					idx = i + 1;
				}
			}

			combo.SelectedIndex = idx;

			combo.SelectedIndexChanged += (s, a) => record.SetValue(obj, combo.SelectedItem.Tag);

			return combo;
		}

		private Widget CreateCustomEditor(Record record, object obj)
		{
			var loadCustomEditor = CreateLoadCustomEditor(record, obj);
			if (loadCustomEditor != null)
			{
				return loadCustomEditor;
			}

			var selectCustomEditor = CreateCustomSelectEditor(record, obj);
			if (selectCustomEditor != null)
			{
				return selectCustomEditor;
			}

			/*			// IMaterial gets special treatment
						var materialEditor = CreateMaterialEditor(record, obj);
						if (materialEditor != null)
						{
							return materialEditor;
						}*/

			return null;
		}

		private bool SetTabByName(TabControl tabControl, string filePath)
		{
			for (var i = 0; i < tabControl.Items.Count; ++i)
			{
				var tabItem = tabControl.Items[i];
				var tabInfo = (TabInfo)tabItem.Tag;
				if (tabInfo.FilePath == filePath)
				{
					tabControl.SelectedIndex = i;
					return true;
				}
			}

			return false;
		}

		private void UpdateStackPanelEditor()
		{
			_panelScenes.Visible = _tabControlScenes.Items.Count > 0;
		}

		public void OpenFile(string file)
		{
			try
			{
				if (!File.Exists(file))
				{
					return;
				}

				if (file.EndsWith(".scene"))
				{
					if (SetTabByName(_tabControlScenes, file))
					{
						return;
					}

					// Create scene widget
					var sceneWidget = new SceneWidget
					{
						HorizontalAlignment = HorizontalAlignment.Stretch,
						VerticalAlignment = VerticalAlignment.Stretch,
					};

					// Load scene
					sceneWidget.LoadScene(file, false);

					var panel = new Panel();
					var buttonsPanel = new HorizontalStackPanel
					{
						Id = ButtonsPanelId,
						Left = 2,
						Top = 2,
						Spacing = 8
					};

					panel.Widgets.Add(sceneWidget);
					panel.Widgets.Add(buttonsPanel);

					var tabInfo = new TabInfo(file);

					var tabItem = new TabItem
					{
						Text = tabInfo.Title,
						Content = panel,
						Tag = tabInfo
					};

					tabInfo.TitleChanged += (s, a) => tabItem.Text = tabInfo.Title;

					_tabControlScenes.Items.Add(tabItem);
					_tabControlScenes.SelectedIndex = _tabControlScenes.Items.Count - 1;
				}
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
				return;
			}
		}

		private void OpenCurrentSolutionItem()
		{
			var node = _treeFileSolution.SelectedNode;
			if (node == null || node.Tag == null || !(node.Tag is string))
			{
				return;
			}

			var file = (string)node.Tag;
			OpenFile(file);
		}

		private void CheckHasDefaultMaterial()
		{
			if (_propertyGrid.Object == null)
			{
				return;
			}

			var defaultMaterialProperty = _propertyGrid.Object.GetType().GetPropertyWithAttribute<DefaultMaterialAttribute>();
			if (defaultMaterialProperty == null)
			{
				return;
			}

			// Add set material button
			var buttonSetMaterial = new Button
			{
				Content = new Label
				{
					Text = "Set Material"
				}
			};

			buttonSetMaterial.Click += (s, a) =>
			{
				var dlg = new Dialog
				{
					Title = "Choose Material",
				};

				var panel = new HorizontalStackPanel
				{
					Spacing = 8
				};

				panel.Widgets.Add(new Label
				{
					Text = "Material:"
				});

				var combo = new ComboView();
				combo.Widgets.Add(new Label
				{
					Text = "BlinnPhongMaterial",
					Tag = typeof(BlinnPhongMaterial)
				});

				combo.Widgets.Add(new Label
				{
					Text = "UnlitMaterial",
					Tag = typeof(UnlitMaterial)
				});
				combo.SelectedIndex = 0;
				panel.Widgets.Add(combo);

				dlg.Content = panel;

				dlg.Closed += (s, a) =>
				{
					if (!dlg.Result || _propertyGrid.Object == null)
					{
						return;
					}

					var materialType = (Type)combo.SelectedItem.Tag;
					var material = Activator.CreateInstance(materialType);

					var materialProperty = _propertyGrid.Object.GetType().GetPropertyWithAttribute<DefaultMaterialAttribute>();
					if (materialProperty != null)
					{
						materialProperty.SetValue(_propertyGrid.Object, material);
						RebuildProperties();
					}
				};

				dlg.ShowModal(Desktop);
			};

			_panelPropertiesButtons.Widgets.Add(buttonSetMaterial);
		}

		private void CheckModelSelected()
		{
			var asModel = _propertyGrid.Object as NursiaModelNode;
			if (asModel == null || asModel.Model == null)
			{
				return;
			}

			var separator = new HorizontalSeparator();
			SetProportionType(separator, ProportionType.Auto);

			_rightPanel.Widgets.Add(separator);

			_rightPanel.Widgets.Add(_scrollViewerModel);

			_treeModel.RemoveAllSubNodes();

			var root = _treeModel.AddSubNode(new Label
			{
				Text = "Model"
			});
			root.IsExpanded = true;
			root.Tag = asModel;

			if (asModel.Model != null)
			{
				for (var i = 0; i < asModel.Model.Meshes.Length; ++i)
				{
					var mesh = asModel.Model.Meshes[i];
					var name = mesh.Name;
					if (name == null)
					{
						name = mesh.ParentBone.Name;
					}

					if (name == null)
					{
						name = $"Mesh #{i}";
					}

					var meshNode = root.AddSubNode(new Label
					{
						Text = name
					});
					meshNode.IsExpanded = true;

					for (var j = 0; j < mesh.MeshParts.Count; ++j)
					{
						var meshPartNode = meshNode.AddSubNode(new Label
						{
							Text = $"Part #{j}"
						});

						meshPartNode.Tag = asModel.GetMaterial(i, j);
					}
				}

				// Add reset materials button
				var buttonResetMaterials = new Button
				{
					Content = new Label
					{
						Text = "Reset Materials"
					}
				};
				buttonResetMaterials.Click += (s, a) => asModel.SetMaterialsFromModel();

				_panelPropertiesButtons.Widgets.Add(buttonResetMaterials);
			}
		}

		private void UpdateSceneWidgets()
		{
			TerrainInstrument.Terrain = null;

			var tab = _tabControlScenes.SelectedItem;
			if (tab == null)
			{
				return;
			}

			var buttonsGrid = _tabControlScenes.SelectedItem.Content.FindChildById<StackPanel>(ButtonsPanelId);
			buttonsGrid.Widgets.Clear();
			var asCamera = _propertyGrid.Object as Camera;
			if (asCamera != null)
			{
				// Add button to the grid
				var label = new Label
				{
					Text = "Camera View"
				};

				var buttonShowCamera = new ToggleButton
				{
					Id = ButtonCameraViewId,
					Content = label,
				};

				buttonsGrid.Widgets.Add(buttonShowCamera);
			}

			var asTerrain = _propertyGrid.Object as TerrainNode;
			if (asTerrain != null)
			{
				TerrainInstrument.Terrain = asTerrain;

				var terrainEditorPanel = new TerrainEditorPanel(TerrainInstrument);
				buttonsGrid.Widgets.Add(terrainEditorPanel);
			}
		}

		private void _treeFileExplorer_SelectionChanged(object sender, EventArgs e)
		{
			_propertyGrid.Object = _treeFileExplorer.SelectedNode?.Tag;

			_rightPanel.Widgets.Clear();
			_rightPanel.Widgets.Add(_panelProperties);

			_panelPropertiesButtons.Widgets.Clear();

			CheckHasDefaultMaterial();
			CheckModelSelected();
			UpdateSceneWidgets();
		}

		private void _treeModel_SelectionChanged(object sender, EventArgs e)
		{
			if (_treeModel.SelectedNode == null)
			{
				return;
			}

			_propertyGrid.Object = _treeModel.SelectedNode.Tag;
		}

		private void OnCreateNewTerrain(SceneNode root)
		{
			var dialog = new CreateNewTerrainDialog();

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					return;
				}

				// Assign id
				int newId = 0;
				var terrains = CurrentSceneWidget.Scene.Root.QueryByType<TerrainNode>();
				if (terrains.Count > 0)
				{
					newId = (from t in terrains select t.EditorId).Max();
					++newId;
				}

				// Create splat map
				var tabInfo = (TabInfo)_tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Tag;
				var folder = Path.GetDirectoryName(tabInfo.FilePath);

				var splatFile = $"t{newId}_splat.png";
				var splatFilePath = Path.Combine(folder, splatFile);
				var splatMap = new byte[dialog.SplatMapSize * dialog.SplatMapSize * 4];
				for (var i = 0; i < splatMap.Length; ++i)
				{
					splatMap[i] = 255;
				}

				var imageWriter = new ImageWriter();
				using (var stream = File.Create(splatFilePath))
				{
					imageWriter.WritePng(splatMap, dialog.SplatMapSize, dialog.SplatMapSize, ColorComponents.RedGreenBlueAlpha, stream);
				}

				// Create height field
				var heightFile = $"t{newId}_height.hf";
				var heightFilePath = Path.Combine(folder, heightFile);
				var heightField = new HeightField(256, 256);
				using (var stream = File.Create(heightFilePath))
				{
					heightField.SaveToHf(stream);
				}

				var newTerrain = new TerrainNode()
				{
					EditorId = newId,
					HeightFieldPath = heightFile,
					PatchSize = heightField.Columns / 8
				};

				var material = new TerrainMaterial
				{
					WeightMap1Path = splatFile
				};

				newTerrain.Material = material;

				var assetManager = AssetManager.CreateFileAssetManager(folder);
				newTerrain.Load(assetManager);

				root.Children.Add(newTerrain);

				RefreshExplorer(newTerrain);
			};

			dialog.ShowModal(Desktop);
		}

		private void _treeFileExplorer_TouchUp(object sender, EventArgs e)
		{
			if (_explorerTouchDown != TouchDownType.Explorer)
			{
				return;
			}

			_explorerTouchDown = TouchDownType.None;

			var treeNode = _treeFileExplorer.SelectedNode;
			if (treeNode == null || treeNode.Tag == null)
			{
				return;
			}

			var sceneNode = (SceneNode)treeNode.Tag;

			var contextMenuOptions = new List<Tuple<string, Action>>();
			contextMenuOptions.Add(new Tuple<string, Action>("Add New Node...", () =>
			{
				var dialog = new AddNewSceneNodeDialog
				{
					RequireFilename = false
				};

				dialog.Closed += (s, a) =>
				{
					if (!dialog.Result)
					{
						// "Cancel" or Escape
						return;
					}

					// "Ok" or Enter
					try
					{
						if (dialog.Type == typeof(TerrainNode))
						{
							OnCreateNewTerrain(sceneNode);
						}
						else
						{
							var newScene = (SceneNode)Activator.CreateInstance(dialog.Type);

							var materialProp = newScene.GetType().GetPropertyWithAttribute<DefaultMaterialAttribute>();
							if (materialProp != null)
							{
								materialProp.SetValue(newScene, new BlinnPhongMaterial());
							}

							sceneNode.Children.Add(newScene);

							RefreshExplorer(newScene);
						}
					}
					catch (Exception ex)
					{
						var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
						dialog.ShowModal(Desktop);
					}
				};

				dialog.ShowModal(Desktop);
			}));

			contextMenuOptions.Add(new Tuple<string, Action>("Delete Node", () =>
			{
				sceneNode.RemoveFromParent();
				InvalidateCurrentItem();
				RefreshExplorer(null);
			}));

			Desktop.BuildContextMenu(contextMenuOptions);
		}

		private void _treeFileSolution_TouchUp(object sender, EventArgs e)
		{
			if (_explorerTouchDown != TouchDownType.FileSystem)
			{
				return;
			}

			_explorerTouchDown = TouchDownType.None;

			var treeNode = _treeFileSolution.SelectedNode;
			var path = treeNode.Tag as string;
			if (treeNode == null || path == null)
			{
				return;
			}

			if (!Directory.Exists(path))
			{
				return;
			}

			var contextMenuOptions = new List<Tuple<string, Action>>();
			contextMenuOptions.Add(new Tuple<string, Action>("Add New Scene...", () =>
			{
				var dialog = new AddNewSceneNodeDialog
				{
					RequireFilename = true
				};

				dialog.Closed += (s, a) =>
				{
					if (!dialog.Result)
					{
						// "Cancel" or Escape
						return;
					}

					// "Ok" or Enter
					try
					{
						var newScene = (SceneNode)Activator.CreateInstance(dialog.Type);

						var fileName = Path.ChangeExtension(dialog.Name, "scene");
						var fullPath = Path.Combine(path, fileName);

						var stored = new Scene(newScene);
						stored.SaveToFile(fullPath);
						RefreshProject(treeNode, path);
						OpenFile(fullPath);
					}
					catch (Exception ex)
					{
						var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
						dialog.ShowModal(Desktop);
					}
				};

				dialog.ShowModal(Desktop);
			}));

			Desktop.BuildContextMenu(contextMenuOptions);
		}


		private void RefreshProject(TreeViewNode root, string path)
		{
			// Update root node
			root.RemoveAllSubNodes();
			root.Tag = path;

			// Add subfolders
			var folders = Directory.GetDirectories(path);
			foreach (var folder in folders)
			{
				var subFolderNode = root.AddSubNode(new Label
				{
					Text = Path.GetFileName(folder)
				});

				RefreshProject(subFolderNode, folder);
			}

			// Scenes
			var files = Directory.GetFiles(path, "*.scene");
			foreach (var file in files)
			{
				var node = root.AddSubNode(new Label
				{
					Text = Path.GetFileName(file),
				});

				node.Tag = file;
			}

			root.IsExpanded = true;
		}

		public void OpenFolder(string path)
		{
			try
			{
				if (!string.IsNullOrEmpty(path))
				{
					// Nrs.EffectsSource = new DynamicEffectsSource(Path.GetDirectoryName(path));
					_treeFileSolution.RemoveAllSubNodes();

					var subFolderNode = _treeFileSolution.AddSubNode(new Label
					{
						Text = Path.GetFileName(path)
					});

					RefreshProject(subFolderNode, path);
				}

				StudioGame.Instance.Window.Title = State.StateFilePath;
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		private void ProcessSave(Scene scene, string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}

			try
			{
				scene.SaveToFile(filePath);
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		/*		private void Save(bool setFileName)
				{
					if (string.IsNullOrEmpty(FilePath) || setFileName)
					{
						var dlg = new FileDialog(FileDialogMode.SaveFile)
						{
							Filter = "*.scene"
						};

						if (!string.IsNullOrEmpty(FilePath))
						{
							dlg.FilePath = FilePath;
						}

						dlg.ShowModal(Desktop);

						dlg.Closed += (s, a) =>
						{
							if (dlg.Result)
							{
								ProcessSave(dlg.FilePath);
							}
						};
					}
					else
					{
						ProcessSave(FilePath);
					}
				}*/

		private void UpdateTreeNodeId(TreeViewNode node)
		{
			var sceneNode = (SceneNode)node.Tag;
			var label = (Label)node.Content;
			label.Text = $"{sceneNode.GetType().Name} (#{sceneNode.Id})";
		}

		private TreeViewNode RecursiveAddToExplorer(ITreeViewNode treeViewNode, SceneNode sceneNode)
		{
			var label = new Label();
			var newNode = treeViewNode.AddSubNode(label);
			newNode.IsExpanded = true;
			newNode.Tag = sceneNode;

			UpdateTreeNodeId(newNode);

			foreach (var child in sceneNode.Children)
			{
				RecursiveAddToExplorer(newNode, child);
			}

			return newNode;
		}

		private void RefreshExplorer(SceneNode selectedNode)
		{
			_treeFileExplorer.RemoveAllSubNodes();
			if (_tabControlScenes.SelectedIndex == null ||
				_tabControlScenes.SelectedIndex < 0 ||
				_tabControlScenes.SelectedIndex >= _tabControlScenes.Items.Count)
			{
				return;
			}

			var sceneWidget = _tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Content.FindChild<SceneWidget>();
			RecursiveAddToExplorer(_treeFileExplorer, sceneWidget.Scene.Root);
			_treeFileExplorer.SelectedNode = _treeFileExplorer.FindNode(n => n.Tag == selectedNode);
		}

		public void RefreshLibrary()
		{
		}

		public void InvalidateCurrentItem()
		{
			if (_tabControlScenes.SelectedIndex == null)
			{
				return;
			}

			var tabInfo = (TabInfo)_tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Tag;
			tabInfo.Dirty = true;
		}

		private void SaveItem(int tabIndex)
		{
			var tab = _tabControlScenes.Items[tabIndex];
			var sceneWidget = tab.Content.FindChild<SceneWidget>();

			var tabInfo = (TabInfo)tab.Tag;

			sceneWidget.Scene.SaveToFile(tabInfo.FilePath);

			var terrains = sceneWidget.Scene.Root.QueryByType<TerrainNode>();
			if (terrains != null)
			{
				var assetManager = AssetManager.CreateFileAssetManager(Path.GetDirectoryName(tabInfo.FilePath));

				var method = assetManager.GetType().GetMethod("BuildFullPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				foreach (var terrain in terrains)
				{
					if (terrain.HeightField != null && !string.IsNullOrEmpty(terrain.HeightFieldPath))
					{
						var path = (string)method.Invoke(assetManager, new object[] { terrain.HeightFieldPath });
						path = path.Replace('/', Path.DirectorySeparatorChar);

						using (var stream = File.Create(path))
						{
							terrain.HeightField.SaveToHf(stream);
						}
					}

					var material = terrain.Material as TerrainMaterial;
					if (material != null && material.WeightMap1 != null && !string.IsNullOrEmpty(material.WeightMap1Path))
					{
						var path = (string)method.Invoke(assetManager, new object[] { material.WeightMap1Path });
						path = path.Replace('/', Path.DirectorySeparatorChar);

						var data = new byte[material.WeightMap1.Width * material.WeightMap1.Height * 4];
						material.WeightMap1.GetData(data);
						var imageWriter = new ImageWriter();
						using (var stream = File.Create(path))
						{
							imageWriter.WritePng(data, material.WeightMap1.Width, material.WeightMap1.Height, ColorComponents.RedGreenBlueAlpha, stream);
						}
					}
				}
			}

			tabInfo.Dirty = false;
		}

		private void SaveCurrentItem()
		{
			if (_tabControlScenes.SelectedIndex == null)
			{
				return;
			}

			SaveItem(_tabControlScenes.SelectedIndex.Value);
		}

		private void _menuItemSaveEverything_Selected(object sender, EventArgs e)
		{
			for (var i = 0; i < _tabControlScenes.Items.Count; ++i)
			{
				SaveItem(i);
			}
		}

		private void _menuItemReloadCurrentItem_Selected(object sender, EventArgs e)
		{
			if (_tabControlScenes.SelectedIndex == null)
			{
				return;
			}

			var tab = _tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value];
			var sceneWidget = tab.Content.FindChild<SceneWidget>();
			var tabInfo = (TabInfo)tab.Tag;

			if (tabInfo.Dirty)
			{
				var dialog = Dialog.CreateMessageBox("Confirm Reload", "The scenes has changes. The reloading would discard it. Are you sure?");

				dialog.Closed += (s, a) =>
				{
					if (!dialog.Result)
					{
						return;
					}

					sceneWidget.LoadScene(tabInfo.FilePath, false);
					RefreshExplorer(null);
					tabInfo.Dirty = false;
				};

				dialog.ShowModal(Desktop);
			}
			else
			{
				sceneWidget.LoadScene(tabInfo.FilePath, false);
				RefreshExplorer(null);
			}
		}

		private void UpdateEditorOptions()
		{
			NursiaEditorOptions.ShowGrid = _buttonGrid.IsToggled;
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			if (_panelStatistics.Visible == false || CurrentSceneWidget == null)
			{
				return;
			}

			var stats = CurrentSceneWidget.RenderStatistics;

			_labelFps.Text = CurrentSceneWidget.FramesPerSecond.ToString();
			_labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
			_labelDrawCalls.Text = stats.DrawCalls.ToString();
			_labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();
			_labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
			_labelPasses.Text = stats.Passes.ToString();

			var pos = CurrentCamera.GlobalTransform.Translation;
			_labelCameraPosition.Text = $"{pos.X}, {pos.Y}, {pos.Z}";
		}

		private static HeightField LoadHeightField(AssetManager assetManager, string path)
		{
			HeightField heightField;
			using (var stream = assetManager.Open(path))
			{
				heightField = HeightField.FromStreamHf(stream);
			}

			return heightField;
		}
	}
}