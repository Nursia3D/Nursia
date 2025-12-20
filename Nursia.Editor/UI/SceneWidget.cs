using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Nursia.Editor.Utility;
using Nursia.Env;
using Nursia.Materials;
using Nursia.Rendering;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using Nursia.SceneGraph.Primitives;
using Nursia.Utilities;
using Nursia.Utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nursia.Editor.UI
{
	public class SceneWidget : Widget
	{
		private const int GridSize = 200;

		private static readonly BaseLight[] _defaultLights = new BaseLight[]
		{
			// Front light
			new DirectLight { Rotation = new Vector3(45, 45, 0), CastsShadow = false },

			// Back light
			new DirectLight { Rotation = new Vector3(225, 45, 0), CastsShadow = false }
		};

		private Scene _scene;
		private readonly SceneNode _root = new SceneNode();
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private CameraInputController _controller;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private MeshNode _gridMesh;
		private DrMeshPart _waterMarker;
		private DrModel _modelMarker;
		private Vector3? _touchDownStart;
		private readonly bool[] _keysDown = new bool[256];
		private readonly SpriteBatch _spriteBatch;

		public Scene Scene
		{
			get => _scene;
		}

		public ForwardRenderer Renderer { get => _renderer; }
		public RenderStatistics RenderStatistics;
		public int FramesPerSecond => _fpsCounter.FramesPerSecond;

		private MeshNode GridMesh
		{
			get
			{
				if (_gridMesh == null)
				{
					var vertices = new List<Vector3>();
					var indices = new List<ushort>();

					ushort idx = 0;
					for (var x = -GridSize; x <= GridSize; ++x)
					{
						vertices.Add(new Vector3(x, 0, -GridSize));
						vertices.Add(new Vector3(x, 0, GridSize));

						indices.Add(idx);
						++idx;
						indices.Add(idx);
						++idx;
					}

					for (var z = -GridSize; z <= GridSize; ++z)
					{
						vertices.Add(new Vector3(-GridSize, 0, z));
						vertices.Add(new Vector3(GridSize, 0, z));

						indices.Add(idx);
						++idx;
						indices.Add(idx);
						++idx;
					}

					var mesh = new DrMeshPart(Nrs.GraphicsDevice, vertices.ToArray(), indices.ToArray(), PrimitiveType.LineList);

					_gridMesh = new MeshNode
					{
						Mesh = mesh,
						Material = new UnlitMaterial
						{
							DiffuseColor = Color.Green,
							CastsShadows = false
						},
					};
				}

				return _gridMesh;
			}
		}


		private static bool IsMouseLeftButtonDown
		{
			get
			{
				var mouseState = Mouse.GetState();
				return (mouseState.LeftButton == ButtonState.Pressed);
			}
		}

		public SceneWidget()
		{
			ClipToBounds = true;
			AcceptsKeyboardFocus = true;

			_spriteBatch = new SpriteBatch(Nrs.GraphicsDevice);
		}

		private Vector3? CalculateTerrainMarkerPosition()
		{
			var terrain = StudioGame.MainForm.TerrainInstrument.Terrain;
			if (terrain == null)
			{
				return null;
			}

			// Build viewport
			var bounds = ActualBounds;
			var p = ToGlobal(bounds.Location);
			bounds.X = p.X;
			bounds.Y = p.Y;
			var viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);

			// Determine marker position
			var nearPoint = new Vector3(Desktop.MousePosition.X, Desktop.MousePosition.Y, 0);
			var farPoint = new Vector3(Desktop.MousePosition.X, Desktop.MousePosition.Y, 1);

			var camera = _scene.Camera;

			var view = camera.View;
			var projection = camera.Projection;
			nearPoint = viewport.Unproject(nearPoint, projection, view, Matrix.Identity);
			farPoint = viewport.Unproject(farPoint, projection, view, Matrix.Identity);

			var direction = (farPoint - nearPoint);
			direction.Normalize();

			var ray = new Ray(nearPoint, direction);
			return terrain.FindPosition(ray);
		}

		private void UpdateMarker()
		{
			if (Scene == null)
			{
				return;
			}

			Nrs.EditorSettings.TerrainMarkerPosition = CalculateTerrainMarkerPosition();
			Nrs.EditorSettings.TerrainMarkerRadius = StudioGame.MainForm.TerrainInstrument.Radius;
			// Scene.Marker.Radius = Instrument.Radius;
		}

		public override void OnKeyDown(Keys k)
		{
			base.OnKeyDown(k);

			_keysDown[(int)k] = true;
		}

		public override void OnKeyUp(Keys k)
		{
			base.OnKeyUp(k);

			_keysDown[(int)k] = false;
		}

		public override void OnLostKeyboardFocus()
		{
			base.OnLostKeyboardFocus();

			for (var i = 0; i < _keysDown.Length; i++)
			{
				_keysDown[i] = false;
			}
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			if (Scene == null || Scene.Root == null)
			{
				return;
			}

			_controller.Update();

			var bounds = ActualBounds;

			var p = ToGlobal(bounds.Location);
			bounds.X = p.X;
			bounds.Y = p.Y;

			// Save scissor as it would be destroyed on exception
			var device = Nrs.GraphicsDevice;
			var scissor = device.ScissorRectangle;

			_root.Children.Clear();

			try
			{
				var hasLight = Scene.Root.QueryFirstByType<BaseLight>() != null;
				if (!hasLight)
				{
					// Add default lights
					foreach (var light in _defaultLights)
					{
						_root.Children.Add(light);
					}
				}

				var camera = StudioGame.MainForm.CurrentCamera;

				_root.Children.Add(Scene.Root);

				if (NursiaEditorOptions.ShowGrid)
				{
					_root.Children.Add(GridMesh);
				}

				// Selected object
				var selectionNode = _3DUtils.GetSelectionNode(StudioGame.MainForm.SelectedObject as SceneNode);
				if (selectionNode != null && camera == Scene.Camera)
				{
					_root.Children.Add(selectionNode);
				}

				// Draw lights' icons and gizmos
				Scene.Root.Traverse(AddGizmos);

				// Draw main target
				var target = _renderer.RenderToTarget(_root, camera, _scene.RenderEnvironment, bounds.Width, bounds.Height);
				_spriteBatch.Begin(SpriteSortMode.Immediate, blendState: BlendState.Opaque);
				_spriteBatch.Draw(target, bounds, Color.White);
				_spriteBatch.End();

				DebugShapeRenderer.Draw(camera.View, camera.Projection);

				RenderStatistics = _renderer.Statistics;

				var m = Editor.Resources.ModelAxises;
				var c = (Camera)Scene.Camera.Clone();

				// Make the gizmo placed always in front of the camera
				c.Translation = Vector3.Zero;
				var direction = c.GlobalTransform.Forward;
				direction.Normalize();
				m.Translation = direction * 2.5f;

				// Draw axises
				target = _renderer.RenderToTarget(m, c, 160, 160);
				_spriteBatch.Begin(SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);
				_spriteBatch.Draw(target, new Rectangle(bounds.Right - 160, bounds.Top, 160, 160), Color.White);
				_spriteBatch.End();

				UpdateMarker();

				_fpsCounter.OnFrameDrawn();
			}
			catch (Exception ex)
			{
				Nrs.GraphicsDevice.ScissorRectangle = scissor;
				var font = Editor.Resources.ErrorFont;
				var message = ex.ToString();
				var sz = font.MeasureString(message);

				bounds = ActualBounds;
				var pos = new Vector2(bounds.X + (bounds.Width - sz.X) / 2,
					bounds.Y + (bounds.Height - sz.Y) / 2);

				pos.X = (int)pos.X;
				pos.Y = (int)pos.Y;
				context.DrawString(font, message, pos, Color.Red);
			}
		}

		private void AddGizmos(SceneNode n)
		{
			var asCamera = n as Camera;
			if (asCamera != null)
			{
				var gizmo = asCamera.GetGizmo(Editor.Resources.IconCamera, null);
				_root.Children.Add(gizmo);

				var far = Math.Min(asCamera.FarPlane, 5);
				var frustum = new BoundingFrustum(asCamera.View * asCamera.CalculateProjection(asCamera.NearPlane, far));
				DebugShapeRenderer.AddBoundingFrustum(frustum, Color.Brown);
			}

			var asDirectLight = n as DirectLight;
			if (asDirectLight != null)
			{
				var gizmo = asDirectLight.GetGizmo(Editor.Resources.IconDirectionalLight, asDirectLight.Color);
				_root.Children.Add(gizmo);

				SceneNode dir;
				if (gizmo.Tag == null)
				{
					dir = new SceneNode();
					dir.Children.Add(Editor.Resources.DirectLightArrow.Clone());
					gizmo.Tag = dir;
				}
				else
				{
					dir = (SceneNode)gizmo.Tag;
				}

				var cone = (MeshNodeBase)dir.QueryFirstById("_cone");
				((UnlitMaterial)cone.Material).DiffuseColor = asDirectLight.Color;

				var cylinder = (MeshNodeBase)dir.QueryFirstById("_cylinder");
				((UnlitMaterial)cylinder.Material).DiffuseColor = asDirectLight.Color;

				dir.GlobalTransform = asDirectLight.GlobalTransform;
				_root.Children.Add(dir);
			}

			var asPointLight = n as PointLight;
			if (asPointLight != null)
			{
				var gizmo = asPointLight.GetGizmo(Editor.Resources.IconDirectionalLight, asPointLight.Color);
				_root.Children.Add(gizmo);

				Sphere sphere;
				if (gizmo.Tag == null)
				{
					sphere = (Sphere)Editor.Resources.PointLightSphere.Clone();
					gizmo.Tag = sphere;
				}
				else
				{
					sphere = (Sphere)gizmo.Tag;
				}

				var c = asPointLight.Color;

				var material = ((UnlitMaterial)sphere.Material);
				material.DiffuseColor = new Color(c.R, c.G, c.B, (byte)32);

				sphere.Translation = n.GlobalTransform.Translation;
				sphere.Scale = new Vector3(asPointLight.Range);
				_root.Children.Add(sphere);
			}

			var asSpotLight = n as SpotLight;
			if (asSpotLight != null)
			{
				var gizmo = asSpotLight.GetGizmo(Editor.Resources.IconDirectionalLight, asSpotLight.Color);
				_root.Children.Add(gizmo);

				DebugShapeRenderer.AddBoundingFrustum(asSpotLight.Frustum, asSpotLight.Color);
			}
		}

		private void UpdateTerrainHeight(TerrainInstrument instrument, int x, int z, float power)
		{
			var terrain = instrument.Terrain;
			var height = terrain.HeightField.GetHeight(x, z);

			power *= 0.05f;

			height += power;

			// Clamp by MinHeight/MaxHeigt
			var scaledHeight = height * terrain.TerrainSize.Y;
			scaledHeight = MathHelper.Clamp(scaledHeight, instrument.MinHeight, instrument.MaxHeight);
			height = scaledHeight / terrain.TerrainSize.Y;

			// Set the final height
			terrain.HeightField.SetHeight(x, z, height);
		}

		private void ApplyLowerRaise()
		{
			var mp = CalculateTerrainMarkerPosition();
			if (mp == null)
			{
				return;
			}

			var markerPos = mp.Value;

			var instrument = StudioGame.MainForm.TerrainInstrument;
			var power = instrument.Power;
			var terrain = instrument.Terrain;
			var radius = instrument.Radius;

			// Convert marker pos to model space
			markerPos = Vector3.Transform(markerPos, terrain.InverseGlobalTransform);

			// Now determine top left and bottom right in weightmap space
			var topLeft = terrain.Terrain.WorldToHeightField(new Vector2(markerPos.X - radius, markerPos.Z - radius));
			var bottomRight = terrain.Terrain.WorldToHeightField(new Vector2(markerPos.X + radius, markerPos.Z + radius));

			var rect = new Rectangle((int)Math.Min(topLeft.X, bottomRight.X),
				(int)Math.Min(topLeft.Y, bottomRight.Y),
				(int)Math.Abs(topLeft.X - bottomRight.X) + 1,
				(int)Math.Abs(topLeft.Y - bottomRight.Y) + 1);

			var heightRadius = rect.Width / 2.0f;

			for (var x = rect.X; x <= rect.Right; ++x)
			{
				for (var y = rect.Y; y <= rect.Bottom; ++y)
				{
					var dist = Vector2.Distance(new Vector2(x, y), rect.Center.ToVector2());
					if (dist > heightRadius)
					{
						//continue;
					}

					switch (instrument.Type)
					{
						case TerrainInstrumentType.RaiseTerrain:
							UpdateTerrainHeight(instrument, x, y, power);
							break;
						case TerrainInstrumentType.LowerTerrain:
							UpdateTerrainHeight(instrument, x, y, -power);
							break;
					}
				}
			}

			StudioGame.MainForm.InvalidateCurrentItem();
		}

		private void UpdateTerrainSplatMap(TerrainInstrument instrument, int x, int z)
		{
			var channelIndex = instrument.Type.GetChannelIndex();

			var c = instrument.GetSplatData(x, z);
			var power = instrument.Power * 0.5f;

			for (var i = 0; i < 4; ++i)
			{
				var splatValue = c.GetChannelValue(i) / 255.0f;

				if (i != channelIndex)
				{
					splatValue -= power;
				}
				else
				{
					splatValue += power;
				}

				splatValue = MathHelper.Clamp(splatValue, 0.0f, 1.0f);

				c.SetChannelValue(i, (byte)(splatValue * 255.0f));
			}

			instrument.SetSplatData(x, z, c);
		}


		private void ApplyTerrainPaint()
		{
			var mp = CalculateTerrainMarkerPosition();
			if (mp == null)
			{
				return;
			}

			var markerPos = mp.Value;

			var instrument = StudioGame.MainForm.TerrainInstrument;
			var power = instrument.Power;
			var radius = instrument.Radius;

			var terrain = instrument.Terrain;

			// Convert marker pos to model space
			markerPos = Vector3.Transform(markerPos, terrain.InverseGlobalTransform);

			// Now determine top left and bottom right in weightmap space
			var topLeft = terrain.Terrain.WorldToWeightMapNormalized(new Vector2(markerPos.X - radius, markerPos.Z - radius));
			var bottomRight = terrain.Terrain.WorldToWeightMapNormalized(new Vector2(markerPos.X + radius, markerPos.Z + radius));

			var rect = new Rectangle((int)(Math.Min(topLeft.X, bottomRight.X) * instrument.SplatWidth),
				(int)(Math.Min(topLeft.Y, bottomRight.Y) * instrument.SplatHeight),
				(int)(Math.Abs(topLeft.X - bottomRight.X) * instrument.SplatWidth),
				(int)(Math.Abs(topLeft.Y - bottomRight.Y) * instrument.SplatHeight));
			var weightRadius = rect.Width / 2;

			for (var x = rect.X; x <= rect.Right; ++x)
			{
				for (var y = rect.Y; y <= rect.Bottom; ++y)
				{
					var dist = Vector2.Distance(new Vector2(x, y), rect.Center.ToVector2());
					if (dist > weightRadius)
					{
						continue;
					}

					UpdateTerrainSplatMap(instrument, x, y);
				}
			}

			instrument.UpdateSplatTexture();
			StudioGame.MainForm.InvalidateCurrentItem();
		}



		public override void OnMouseMoved()
		{
			base.OnMouseMoved();

			/*if (Instrument.Type == InstrumentType.Model)
			{
				if (Scene.Marker.Position != null)
				{
					if (_modelMarker == null || _modelMarker != Instrument.Model)
					{
						_modelMarker = Instrument.Model;
					}

					var pos = Scene.Marker.Position.Value;
					pos.Y = -_modelMarker.BoundingBox.Min.Y;
					pos.Y += Scene.Terrain.GetHeight(pos.X, pos.Z);

					_modelMarker.Transform = Matrix.CreateTranslation(pos);
				} else
				{
					_modelMarker = null;
				}
			}*/
		}

		public override void OnMouseLeft()
		{
			base.OnMouseLeft();

			_modelMarker = null;
		}

		private void ApplyTerrainInstrument()
		{
			var instrument = StudioGame.MainForm.TerrainInstrument;
			if (instrument.Terrain == null)
			{
				return;
			}

			if (instrument.Type == TerrainInstrumentType.RaiseTerrain || instrument.Type == TerrainInstrumentType.LowerTerrain)
			{
				ApplyLowerRaise();
			}
			else
			{
				ApplyTerrainPaint();
			}
		}

		public override void OnTouchDown()
		{
			base.OnTouchDown();

			if (!IsMouseLeftButtonDown)
			{
				return;
			}

			ApplyTerrainInstrument();
		}

		public override void OnTouchMoved()
		{
			base.OnTouchMoved();

			if (!IsMouseLeftButtonDown)
			{
				return;
			}

			ApplyTerrainInstrument();
		}

		public void LoadScene(string path, bool keepCameraPosition)
		{
			// Load scene
			var folder = Path.GetDirectoryName(path);
			var file = Path.GetFileName(path);
			var assetManager = AssetManager.CreateFileAssetManager(folder);

			var scene = assetManager.LoadStoredScene(file);

			if (keepCameraPosition && Scene != null)
			{
				// Copy current camera to the new
				scene.Camera.CopyViewParams(Scene.Camera);
			}

			_scene = scene;
			_controller = new CameraInputController(_scene.Camera);
		}
	}
}
