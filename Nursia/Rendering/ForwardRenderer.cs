using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Env;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using Nursia.Shadows;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	/// <summary>
	/// Implements forward rendering for scenes with support for lighting, shadows, and reflections.
	/// </summary>
	public partial class ForwardRenderer : ILightsBatch
	{
		private enum RenderPassType
		{
			Color,
			DepthBuffer,
			ShadowMap
		}

		private class LightsComparer : IComparer<BaseLight>
		{
			public static readonly LightsComparer Instance = new LightsComparer();
			public int Compare(BaseLight x, BaseLight y)
			{
				if (!(x is DirectLight) && y is DirectLight)
				{
					return 1;
				}

				return 0;
			}
		}

		private static readonly Color[] CascadesColors = new Color[]
		{
			Color.Red,
			Color.Yellow,
			Color.Blue,
			Color.White
		};

		private RenderStatistics _statistics;

		private DepthStencilState _oldDepthStencilState;
		private RasterizerState _oldRasterizerState;
		private BlendState _oldBlendState;
		private Viewport _oldViewport;
		private readonly RenderBatchShadowMap _batchShadowMap = new RenderBatchShadowMap();
		private readonly RenderBatchMain _batchMain = new RenderBatchMain();
		private readonly RenderBatchReflection _batchReflection = new RenderBatchReflection();
		private readonly RenderTarget2DPool _renderTargetPool2D = new RenderTarget2DPool();
		private RenderTarget2D _screenBuffer;
		private readonly Camera _tempCamera = new Camera();
		private readonly List<DirectLightCSMData> _directLightCSM = new List<DirectLightCSMData>();
		private readonly List<DirectLightCSMData> _directLightCSMReflection = new List<DirectLightCSMData>();
		private readonly ObjectPool<DirectLightCSMData> _csmPool = new ObjectPool<DirectLightCSMData>(() => new DirectLightCSMData());

		/// <summary>
		/// Gets the list of lights that affect the current render.
		/// </summary>
		public List<BaseLight> Lights { get; } = new List<BaseLight>();

		/// <summary>
		/// Gets rendering statistics for the last render operation.
		/// </summary>
		public RenderStatistics Statistics => _statistics;

		static ForwardRenderer()
		{
			SetEffectLevelParameterSetters();
			SetMeshPartLevelParameterSetters();
		}

		/// <summary>
		/// Initializes a new instance of the ForwardRenderer class.
		/// </summary>
		public ForwardRenderer()
		{
			_bindingContext = new EffectLevelBindingContext(this);
		}

		private void StoreState()
		{
			var device = Nrs.GraphicsDevice;
			_oldDepthStencilState = device.DepthStencilState;
			_oldRasterizerState = device.RasterizerState;
			_oldBlendState = device.BlendState;
			_oldViewport = device.Viewport;
		}

		private void RestoreRenderTarget()
		{
			var device = Nrs.GraphicsDevice;
			device.SetRenderTarget(null);
			device.Viewport = _oldViewport;
		}

		private void RestoreState(bool resetRenderTarget)
		{
			var device = Nrs.GraphicsDevice;
			device.DepthStencilState = _oldDepthStencilState;
			_oldDepthStencilState = null;
			device.RasterizerState = _oldRasterizerState;
			_oldRasterizerState = null;
			device.BlendState = _oldBlendState;
			_oldBlendState = null;
			device.ResetTextures();

			if (resetRenderTarget)
			{
				RestoreRenderTarget();
			}
		}

		private void RenderPass(JobsBatch batch, BaseLight light, RenderPassType passType, bool lightAffectsCheck = true)
		{
			if (batch.Count == 0)
			{
				// No jobs for this pass
				return;
			}

			_bindingContext.SetLight(light);

			var device = Nrs.GraphicsDevice;

			// Store current states
			var currentBlendState = device.BlendState;
			var currentDepthStencilState = device.DepthStencilState;
			var currentRasterizerState = device.RasterizerState;

			// There's a MG bug and DesktopGL backend requires to reset textures periodically
			// Otherwise weird things happen
			// Just in case let's reset texture for other backends too
			device.ResetTextures();

			EffectBinding lastBinding = null;

			// Set effect bindings
			switch (passType)
			{
				case RenderPassType.Color:
					var translucent = light != null ? light.Translucent : false;
					var shadow = Nrs.GraphicsSettings.ShadowType != ShadowType.None && light != null && light.CastsShadow;
					batch.SetTechnique(light.GetTechnique(), shadow, translucent);
					break;
				case RenderPassType.DepthBuffer:
					batch.SetDepthTechnique();
					break;
				case RenderPassType.ShadowMap:
					batch.SetShadowTechnique();
					break;
			}


			// Process each render job
			var jobsRendered = 0;
			foreach (var job in batch.Jobs)
			{
				if (lightAffectsCheck && light != null && !light.AffectsObject(job.WorldBoundingBox))
				{
					continue;
				}

				var effectBinding = job.EffectBinding;
				if (lastBinding == null || effectBinding.BatchId != lastBinding.BatchId)
				{
					// Effect level params
					foreach (var pair in effectBinding.EffectLevelParameters)
					{
						var setter = _effectLevelParameterSetters[(int)pair.Key];
						setter(_bindingContext, pair.Value);
					}

					lastBinding = effectBinding;

					++_statistics.EffectsSwitches;
				}

				job.ModelViewProj = job.Transform * _bindingContext.Camera.ViewProjection;

				_bindingContext.ReflectionTexture = job.ReflectionTexture;

				// Mesh level params
				foreach (var pair in effectBinding.MeshPartLevelParameters)
				{
					var setter = _meshPartLevelParameterSetters[(int)pair.Key];
					setter(job, pair.Value);
				}

				// Material level params
				foreach (var pair in effectBinding.MaterialLevelSetters)
				{
					var v = pair.Value;
					v.Setter(job.Material, v.Parameter);
				}

				var blendStateChanged = false;
				var depthStencilStateChanged = false;
				var rasterizerStateChanged = false;

				if (passType == RenderPassType.Color)
				{
					if (job.Material.BlendState != null)
					{
						device.BlendState = job.Material.BlendState;
						blendStateChanged = true;
					}

					if (job.Material.DepthStencilState != null)
					{
						device.DepthStencilState = job.Material.DepthStencilState;
						depthStencilStateChanged = true;
					}

					if (job.Material.RasterizerState != null)
					{
						device.RasterizerState = job.Material.RasterizerState;
						rasterizerStateChanged = true;
					}
				}

				if (effectBinding.Technique != null)
				{
					effectBinding.Effect.CurrentTechnique = effectBinding.Technique;
				}

				foreach (var pass in effectBinding.Effect.CurrentTechnique.Passes)
				{
					pass.Apply();

					job.Render(device, ref _statistics);
				}

				// Restore states
				if (blendStateChanged)
				{
					device.BlendState = currentBlendState;
				}

				if (depthStencilStateChanged)
				{
					device.DepthStencilState = currentDepthStencilState;
				}

				if (rasterizerStateChanged)
				{
					device.RasterizerState = currentRasterizerState;
				}

				++jobsRendered;
			}

			if (jobsRendered > 0)
			{
				++_statistics.Passes;
			}
		}

		private DirectLightCSMData DirectLightShadowMapRun(DirectLight light, IRenderSource source, Camera camera, bool isMain)
		{
			var device = Nrs.GraphicsDevice;

			// Switch face culling in order to get rid of so called "peter panning"
			device.RasterizerState = RasterizerState.CullClockwise;

			var oldViewport = device.Viewport;
			try
			{
				var size = Nrs.GraphicsSettings.ShadowMapSize.GetSize();

				// Create new one
				var result = _csmPool.Get();
				result.ShadowMap = _renderTargetPool2D.Get(null, "DirectLightShadowMap" + (isMain ? "Main" : "Reflection"),
				   size, size, false, SurfaceFormat.Single, DepthFormat.Depth24);

				// Switch face culling in order to get rid of so called "peter panning"
				device.RasterizerState = RasterizerState.CullClockwise;

				var manager = light.ShadowCascadeManager;
				manager.UpdateShadowMapParameters(camera, light.Direction, result);

				device.SetRenderTarget(result.ShadowMap);

				// Clear the render target to white or all 1's
				// We set the clear to white since that represents the 
				// furthest the object could be away
				device.Clear(Color.White);

				for (var i = 0; i < manager.Cascades; ++i)
				{
					// Batch render jobs
					var shadowCamera = result.Cameras[i];

					_batchShadowMap.BatchScene(source, shadowCamera, null);

					// Render the shadow map
					device.Viewport = manager.GetCascadeViewport(i);

					// Shadow map pass
					_bindingContext.Camera = shadowCamera;
					RenderPass(_batchShadowMap.Batch, null, RenderPassType.ShadowMap);
				}

				return result;
			}
			finally
			{
				device.Viewport = oldViewport;
			}
		}

		private List<DirectLightCSMData> GetDirectLightCSM(bool isMain)
		{
			return isMain ? _directLightCSM : _directLightCSMReflection;
		}


		private void InternalScenePass(IPassBatches batchStorage, Camera camera, bool isMain)
		{
			_bindingContext.Camera = camera;

			var device = Nrs.GraphicsDevice;

			// Opaque runs
			device.BlendState = BlendState.AlphaBlend;
			device.DepthStencilState = DepthStencilState.Default;

			// Lit jobs
			if (Lights.Count == 0)
			{
				// No lights
				RenderPass(batchStorage.OpaqueLit, null, RenderPassType.Color);
			}
			else
			{
				for (var i = 0; i < Lights.Count; i++)
				{
					if (i == 1)
					{
						_bindingContext.NoFog = true;

						device.BlendState = BlendState.Additive;
						device.DepthStencilState = DepthStencilState.DepthRead;
					}

					var light = Lights[i];

					if (light.ShadowMapIndex != null)
					{
						var csms = GetDirectLightCSM(isMain);
						_bindingContext.CSMData = csms[light.ShadowMapIndex.Value];
					}

					RenderPass(batchStorage.OpaqueLit, light, RenderPassType.Color, i != 0);

					_bindingContext.CSMData = null;
				}
			}

			// Unlit jobs
			device.BlendState = BlendState.AlphaBlend;
			device.DepthStencilState = DepthStencilState.Default;
			RenderPass(batchStorage.OpaqueUnlit, null, RenderPassType.Color);

			// Transparent run
			device.RasterizerState = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.DepthRead;
			RenderPass(batchStorage.Transparent, null, RenderPassType.Color);
		}

		private void RenderShadowMaps(IRenderSource source, Camera camera, bool isMain)
		{
			if (Nrs.GraphicsSettings.ShadowType == ShadowType.None)
			{
				return;
			}

			var device = Nrs.GraphicsDevice;
			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.Default;

			foreach (var light in Lights)
			{
				if (!light.CastsShadow)
				{
					continue;
				}

				var asDirectLight = light as DirectLight;
				if (asDirectLight != null)
				{
					var csm = DirectLightShadowMapRun(asDirectLight, source, camera, isMain);
					var csms = GetDirectLightCSM(isMain);
					asDirectLight.ShadowMapIndex = csms.Count;
					csms.Add(csm);
				}
			}
		}

		void ILightsBatch.AddLight(BaseLight light)
		{
			light.ShadowMapIndex = null;
			Lights.Add(light);
		}

		private bool InternalRenderToScreenBuffer(IRenderSource source, Camera camera, RenderEnvironment environment, int width, int height)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (camera == null)
			{
				throw new ArgumentNullException(nameof(camera));
			}

			if (environment == null)
			{
				throw new ArgumentNullException(nameof(environment));
			}

			if (width < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}

			if (height < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(height));
			}

			_bindingContext.NoFog = false;
			_bindingContext.Environment = environment;

			_statistics.Reset();

			var device = Nrs.GraphicsDevice;
			if (width == 0 || height == 0)
			{
				// Can't render
				return false;
			}

			// Update camera viewport
			camera.Width = width;
			camera.Height = height;

			// Set lights
			source.QueryLights(camera, this);

			Lights.Sort(LightsComparer.Instance);

			// Shadow map runs
			RenderShadowMaps(source, camera, true);

			// Batch main
			_batchMain.BatchScene(source, camera, environment);

			if (_batchMain.RequiresDepthBuffer)
			{
				// Depth run
				_bindingContext.Camera = camera;
				_bindingContext.DepthBuffer = _renderTargetPool2D.Get(_bindingContext.DepthBuffer,
					"DepthBuffer", width, height,
					surfaceFormat: SurfaceFormat.Single, depthFormat: DepthFormat.Depth24);
				device.SetRenderTarget(_bindingContext.DepthBuffer);

				// Opaque runs
				device.RasterizerState = RasterizerState.CullCounterClockwise;
				device.BlendState = BlendState.Opaque;
				device.DepthStencilState = DepthStencilState.Default;

				var batchStorage = _batchMain.MainStorage;
				RenderPass(batchStorage.OpaqueUnlit, null, RenderPassType.DepthBuffer);
				RenderPass(batchStorage.OpaqueLit, null, RenderPassType.DepthBuffer);
			}

			_screenBuffer = _renderTargetPool2D.Get(_screenBuffer, "ScreenBuffer",
				width, height,
				depthFormat: DepthFormat.Depth24, usage: RenderTargetUsage.PreserveContents);

			device.SetRenderTarget(_screenBuffer);

			var color = environment.FogEnabled ? environment.FogColor : Color.Transparent;
			device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, color, 1.0f, 0);
			_bindingContext.SetGBufferOffsets(new Point(_screenBuffer.Width, _screenBuffer.Height), new Rectangle(0, 0, _screenBuffer.Width, _screenBuffer.Height));

			// Main Scene Pass
			device.RasterizerState = RasterizerState.CullCounterClockwise;
			InternalScenePass(_batchMain.MainStorage, camera, true);

			// Build reflection textures
			foreach (var job in _batchMain.Reflections)
			{
				// Calculate reflection matrix
				var plane = job.ReflectionPlane.Value;
				plane = Plane.Transform(plane, job.Transform);

				// Setup reflection camera
				_tempCamera.CopyViewParams(camera);
				_tempCamera.ReflectionMatrix = Matrix.CreateReflection(plane);

				// Shadow map runs
				RenderShadowMaps(source, _tempCamera, false);

				_batchReflection.ClipPlane = job.Flags.HasFlag(RenderJobFlags.ClipReflectionPlane) ? plane : (Plane?)null;
				_batchReflection.BatchScene(source, _tempCamera, environment);

				// Create reflection texture and set as the render target
				var reflectionTexture = _renderTargetPool2D.Get(null, "ReflectionTexture", 1024, 1024, depthFormat: DepthFormat.Depth24);
				device.SetRenderTarget(reflectionTexture);
				device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, color, 1.0f, 0);

				device.RasterizerState = RasterizerState.CullClockwise;

				InternalScenePass(_batchReflection, _tempCamera, false);

				/*								using (var stream = File.Create(@"D:\Temp\reflect.png"))
												{
													reflectionTexture.SaveAsPng(stream, reflectionTexture.Width, reflectionTexture.Height);
												}*/

				job.ReflectionTexture = reflectionTexture;
			}

			var sb = Resources.SpriteBatch;
			if (_batchMain.RequiresScreenTexture)
			{
				// Set ScreenTexture
				_bindingContext.ScreenTexture = _renderTargetPool2D.Get(_bindingContext.ScreenTexture, "ScreenTexture", _screenBuffer.Width, _screenBuffer.Height);
				device.SetRenderTarget(_bindingContext.ScreenTexture);

				sb.Begin(SpriteSortMode.Immediate, blendState: BlendState.Opaque);
				sb.Draw(_screenBuffer, Vector2.Zero, Color.White);
				sb.End();
			}

			// Now back to ScreenBuffer
			device.SetRenderTarget(_screenBuffer);

			if (_batchMain.SecondaryStorage.Count > 0)
			{
				// Render secondary jobs
				device.RasterizerState = RasterizerState.CullCounterClockwise;

				InternalScenePass(_batchMain.SecondaryStorage, camera, true);
			}

			// Debug render
			if (Nrs.DebugSettings.DrawBoundingBoxes)
			{
				foreach (var job in _batchMain.AllJobs)
				{
					DebugShapeRenderer.AddBoundingBox(job.WorldBoundingBox, Color.LightGreen);
				}
			}

			switch (Nrs.DebugSettings.VisualizeBuffer)
			{
				case DebugVisualizeBuffer.DepthBuffer:
					if (_bindingContext.DepthBuffer != null)
					{
						sb.Begin();
						sb.Draw(_bindingContext.DepthBuffer, new Rectangle(0, 0, 512, 512), Color.White);
						sb.End();
					}

					break;

				case DebugVisualizeBuffer.ShadowMap:
					if (_directLightCSM.Count > 0)
					{
						sb.Begin();
						sb.Draw(_directLightCSM[0].ShadowMap, new Rectangle(0, 0, 512, 512), Color.White);
						sb.End();
					}

					break;

				case DebugVisualizeBuffer.ReflectionShadowMap:
					if (_directLightCSMReflection.Count > 0)
					{
						sb.Begin();
						sb.Draw(_directLightCSMReflection[0].ShadowMap, new Rectangle(0, 0, 512, 512), Color.White);
						sb.End();
					}

					break;

				case DebugVisualizeBuffer.ReflectionMap:
					if (_batchMain.Reflections.Count > 0)
					{
						sb.Begin();
						sb.Draw(_batchMain.Reflections[0].ReflectionTexture, new Rectangle(0, 0, 512, 512), Color.White);
						sb.End();
					}

					break;
			}

			/*			var terrain = root.QueryFirstByType<TerrainNode>();
						if (terrain != null)
						{
							terrain.DebugDrawBoundingBoxes(camera);
						}*/

			DebugShapeRenderer.Draw(camera.View, camera.Projection);

			/*			using (var stream = File.Create(@"D:\Temp\refract.png"))
						{
							ScreenBuffer.SaveAsPng(stream, ScreenBuffer.Width, ScreenBuffer.Height);
						}*/

			return true;
		}

		private void InternalRender(IRenderSource root, Camera camera, RenderEnvironment environment, int width, int height, bool renderToScreen = true)
		{
			// Set state
			StoreState();

			try
			{
				if (!InternalRenderToScreenBuffer(root, camera, environment, width, height))
				{
					return;
				}

				if (renderToScreen)
				{
					var device = Nrs.GraphicsDevice;
					var sb = Resources.SpriteBatch;

					RestoreRenderTarget();

					sb.Begin(SpriteSortMode.Immediate, blendState: BlendState.Opaque);
					sb.Draw(_screenBuffer, Vector2.Zero, Color.White);
					sb.End();
				}
			}
			finally
			{
				// Restore state
				RestoreState(!renderToScreen);

				// Recycle render targets
				// Reflection maps should be recycled before clearing batches
				// Otherwise _batchMain.Reflections would be empty
				foreach (var job in _batchMain.Reflections)
				{
					if (job.ReflectionTexture != null)
					{
						_renderTargetPool2D.Recycle(job.ReflectionTexture);
						job.ReflectionTexture = null;
					}
				}

				if (renderToScreen && _screenBuffer != null)
				{
					_renderTargetPool2D.Recycle(_screenBuffer);
					_screenBuffer = null;
				}

				if (_bindingContext.DepthBuffer != null)
				{
					_renderTargetPool2D.Recycle(_bindingContext.DepthBuffer);
					_bindingContext.DepthBuffer = null;
				}

				if (_bindingContext.ScreenTexture != null)
				{
					_renderTargetPool2D.Recycle(_bindingContext.ScreenTexture);
					_bindingContext.ScreenTexture = null;
				}

				foreach (var csm in _directLightCSM)
				{
					if (csm.ShadowMap != null)
					{
						_renderTargetPool2D.Recycle(csm.ShadowMap);
					}

					_csmPool.Recycle(csm);
				}
				_directLightCSM.Clear();

				foreach (var csm in _directLightCSMReflection)
				{
					if (csm.ShadowMap != null)
					{
						_renderTargetPool2D.Recycle(csm.ShadowMap);
					}

					_csmPool.Recycle(csm);
				}
				_directLightCSMReflection.Clear();

				_renderTargetPool2D.Update();

				_batchShadowMap.Reset();
				_batchMain.Reset();
				_batchReflection.Reset();
				_bindingContext.Reset();
				Lights.Clear();
			}
		}

		/// <summary>
		/// Renders the scene directly to the screen using the specified environment.
		/// </summary>
		/// <param name="root">The render source containing scene objects.</param>
		/// <param name="camera">The camera defining the view and projection.</param>
		/// <param name="env">The render environment defining lighting, fog, and sky settings.</param>
		public void Render(IRenderSource root, Camera camera, RenderEnvironment env)
		{
			var vp = Nrs.GraphicsDevice.Viewport;
			InternalRender(root, camera, env, vp.Width, vp.Height, true);
		}

		/// <summary>
		/// Renders the scene directly to the screen using the default environment.
		/// </summary>
		/// <param name="root">The render source containing scene objects.</param>
		/// <param name="camera">The camera defining the view and projection.</param>
		public void Render(IRenderSource root, Camera camera) => Render(root, camera, RenderEnvironment.Default);

		/// <summary>
		/// Renders the scene to a render target using the specified environment.
		/// </summary>
		/// <param name="root">The render source containing scene objects.</param>
		/// <param name="camera">The camera defining the view and projection.</param>
		/// <param name="env">The render environment defining lighting, fog, and sky settings.</param>
		/// <param name="width">The width of the render target in pixels.</param>
		/// <param name="height">The height of the render target in pixels.</param>
		/// <returns>A render target containing the rendered scene.</returns>
		public RenderTarget2D RenderToTarget(IRenderSource root, Camera camera, RenderEnvironment env, int width, int height)
		{
			InternalRender(root, camera, env, width, height, false);

			return _screenBuffer;
		}

		/// <summary>
		/// Renders the scene to a render target using the default environment.
		/// </summary>
		/// <param name="root">The render source containing scene objects.</param>
		/// <param name="camera">The camera defining the view and projection.</param>
		/// <param name="width">The width of the render target in pixels.</param>
		/// <param name="height">The height of the render target in pixels.</param>
		/// <returns>A render target containing the rendered scene.</returns>
		public RenderTarget2D RenderToTarget(IRenderSource root, Camera camera, int width, int height) => RenderToTarget(root, camera, RenderEnvironment.Default, width, height);
	}
}