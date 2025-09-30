using AssetManagementBase;
using DigitalRiseModel;
using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Events;
using Myra.Graphics2D.UI;
using Nursia.SceneGraph.Lights;
using Nursia.ModelViewer.UI;
using Nursia.Rendering;
using Nursia.SceneGraph;
using Nursia.Utilities;
using System;
using System.IO;

namespace Nursia.ModelViewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private AnimationController _player = null;
		private CameraInputController _controller;
		private Desktop _desktop;
		private MainPanel _mainPanel;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private bool _isAnimating;
		private string _path;
		private ForwardRenderer _renderer;
		private readonly NursiaModelNode _modelNode = new NursiaModelNode();
		private readonly SceneNode _scene = new SceneNode();

		public ViewerGame(string path)
		{
			_path = path;

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;
		}

		private void ResetAnimation()
		{
			_mainPanel._sliderTime.Value = _mainPanel._sliderTime.Minimum;
		}

		private void LoadModel(string file)
		{
			try
			{
				if (!string.IsNullOrEmpty(file))
				{
					var folder = Path.GetDirectoryName(file);
					var f = Path.GetFileName(file);

					var assetManager = AssetManager.CreateFileAssetManager(folder);

					var model = assetManager.LoadModel(GraphicsDevice, f, ModelLoadFlags.EnsureUVs);
					_modelNode.Model = model;

					_mainPanel._comboAnimations.Widgets.Clear();

					if (model.Animations != null)
					{
						// Default pose
						_mainPanel._comboAnimations.Widgets.Add(new Label());
						foreach (var pair in model.Animations)
						{
							var str = pair.Key;
							if (string.IsNullOrEmpty(str))
							{
								str = "(default)";
							}

							_mainPanel._comboAnimations.Widgets.Add(
								new Label
								{
									Text = str,
									Tag = pair.Value
								});
						}
					}

					if (_mainPanel._comboAnimations.Widgets.Count > 1)
					{
						// First animation
						_mainPanel._comboAnimations.SelectedIndex = 1;
						_mainPanel._comboAnimations.Enabled = true;
						_mainPanel._buttonPlayStop.Enabled = true;
					}
					else
					{
						_mainPanel._comboAnimations.Enabled = false;
						_mainPanel._buttonPlayStop.Enabled = false;
					}
				}

				// Reset camera
				var camera = _controller.Camera;
				if (_modelNode.Model != null)
				{
					var bb = _modelNode.BoundingBox.Value;
					var min = bb.Min;
					var max = bb.Max;
					var center = (min + max) / 2;
					var cameraPosition = new Vector3(center.X, center.Y, center.Z + (max.Z - min.Z) * 3);

					camera.View = Matrix.CreateLookAt(cameraPosition, center, Vector3.Up);

					var size = Math.Max(max.X - min.X, max.Y - min.Y);
					size = Math.Max(size, max.Z - min.Z);

					camera.NearPlane = size / 1000.0f;
					camera.FarPlane = size * 10.0f;
				}
				else
				{
					camera.View = Matrix.CreateLookAt(Vector3.One, Vector3.Zero, Vector3.Up);
				}

				ResetAnimation();
			}
			catch (Exception ex)
			{
				var messageBox = Dialog.CreateMessageBox("Error", ex.Message);
				messageBox.ShowModal(_desktop);
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// UI
			MyraEnvironment.Game = this;
			_mainPanel = new MainPanel();
			_mainPanel._comboAnimations.Widgets.Clear();
			_mainPanel._comboAnimations.SelectedIndexChanged += _comboAnimations_SelectedIndexChanged;

			_mainPanel._comboPlaybackMode.SelectedIndex = 0;
			_mainPanel._comboPlaybackMode.SelectedIndexChanged += (s, a) =>
			{
				_player.PlaybackMode = (PlaybackMode)_mainPanel._comboPlaybackMode.SelectedIndex.Value;
			};

			_mainPanel._sliderSpeed.ValueChanged += (s, a) =>
			{
				_mainPanel._labelSpeed.Text = _mainPanel._sliderSpeed.Value.ToString("0.00");
				_player.Speed = _mainPanel._sliderSpeed.Value;
			};


			_mainPanel._sliderTime.ValueChangedByUser += _sliderTime_ValueChanged;
			_mainPanel._sliderTime.ValueChanged += (s, a) =>
			{
				_mainPanel._labelTime.Text = _mainPanel._sliderTime.Value.ToString("0.00");
			};

			_mainPanel._buttonPlayStop.Click += _buttonPlayStop_Click;

			_mainPanel._checkDrawBoundingBoxes.IsCheckedChanged += (s, a) =>
			{
				Nrs.DebugSettings.DrawBoundingBoxes = _mainPanel._checkDrawBoundingBoxes.IsChecked;
			};

			_mainPanel._checkDisableNormalMapping.IsCheckedChanged += (s, a) =>
			{
				Nrs.DebugSettings.DisableNormalMap = _mainPanel._checkDisableNormalMapping.IsChecked;
			};

			_desktop = new Desktop
			{
				Root = _mainPanel
			};

			Nrs.Game = this;
			_renderer = new ForwardRenderer();

			// Front light
			_scene.Children.Add(new DirectLight { Rotation = new Vector3(45, 45, 0), CastsShadow = false });

			// Back light
			_scene.Children.Add(new DirectLight { Rotation = new Vector3(225, 45, 0), CastsShadow = false });

			_scene.Children.Add(_modelNode);

			_controller = new CameraInputController(new Camera());

			_player = new AnimationController(_modelNode.ModelInstance);
			_player.TimeChanged += (s, a) =>
			{
				if (_player.AnimationClip == null)
				{
					return;
				}

				var k = (float)(_player.Time / _player.AnimationClip.Duration);

				var slider = _mainPanel._sliderTime;
				slider.Value = slider.Minimum + k * (slider.Maximum - slider.Minimum);
			};

			LoadModel(_path);

			// DebugSettings.DrawLights = true;
		}

		private void _buttonPlayStop_Click(object sender, EventArgs e)
		{
			_isAnimating = !_isAnimating;

			var label = (Label)_mainPanel._buttonPlayStop.Content;
			label.Text = _isAnimating ? "Stop" : "Play";
		}

		private void _sliderTime_ValueChanged(object sender, ValueChangedEventArgs<float> e)
		{
			if (!_player.IsPlaying)
			{
				return;
			}

			var k = (e.NewValue - _mainPanel._sliderTime.Minimum) / (_mainPanel._sliderTime.Maximum - _mainPanel._sliderTime.Minimum);
			var passed = _player.AnimationClip.Duration * k;
			_player.Time = passed;
		}

		private void _comboAnimations_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_mainPanel._comboAnimations.SelectedItem == null || string.IsNullOrEmpty(((Label)_mainPanel._comboAnimations.SelectedItem).Text))
			{
				_player.StopClip();
			}
			else
			{
				var clip = (AnimationClip)((Label)_mainPanel._comboAnimations.SelectedItem).Tag;
				if (_mainPanel._checkCrossfade.IsChecked)
				{
					_player.CrossFade(clip.Name, TimeSpan.FromSeconds(0.5f));
				}
				else
				{
					_player.StartClip(clip.Name);
				}
			}

			ResetAnimation();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_controller.Update();

			if (_modelNode.Model != null && _isAnimating)
			{
				_player.Update(gameTime.ElapsedGameTime);
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_renderer.Render(_scene, _controller.Camera);

			_mainPanel._labelFPS.Text = $"FPS: {_fpsCounter.FramesPerSecond}";

			var stats = _renderer.Statistics;
			_mainPanel._labelDrawCalls.Text = stats.DrawCalls.ToString();
			_mainPanel._labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
			_mainPanel._labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
			_mainPanel._labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();
			_mainPanel._labelPasses.Text = stats.Passes.ToString();

			_desktop.Render();

			_fpsCounter.OnFrameDrawn();
		}
	}
}