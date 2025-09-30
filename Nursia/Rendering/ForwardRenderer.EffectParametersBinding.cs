using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Env;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using Nursia.Shadows;
using Nursia.Utilities;
using System;
using System.Diagnostics;

namespace Nursia.Rendering
{
	partial class ForwardRenderer
	{
		private class EffectLevelBindingContext
		{
			private Camera _camera;
			private RenderEnvironment _environment;
			private readonly Stopwatch _elapsedTimer = new Stopwatch();
			public ForwardRenderer Renderer { get; }

			public Camera Camera
			{
				get => _camera;

				set
				{
					_camera = value;
					if (value != null)
					{
						DepthMode = new Vector4(0, 0, 0, 1.0f / Camera.FarPlane);
					}

					UpdateEnvironment();
				}
			}

			public RenderEnvironment Environment
			{
				get => _environment;

				set
				{
					_environment = value;

					UpdateEnvironment();
				}
			}

			public Vector4 FogParams;

			public DirectLight DirectLight { get; private set; }
			public Color LightColor { get; private set; }
			public Vector3 LightDir { get; private set; }
			public Vector4 LightPos { get; private set; }
			public PointLightRamp PointLightRamp { get; private set; }
			public SpotLightRamp SpotLightRamp { get; private set; }

			public Matrix SpotLightMatrix { get; private set; }
			public DirectLightCSMData CSMData { get; set; }
			internal static Vector2 ShadowMapInverseSize
			{
				get
				{
					var size = Nrs.GraphicsSettings.ShadowMapSize.GetSize();
					return new Vector2(1.0f / size, 1.0f / size);
				}
			}

			public Vector3 ShadowParams { get; set; }
			public Vector4 NormalizedShadowDistances { get; set; }
			public Vector4 ShadowDepthFade { get; set; }

			public Vector4 DepthMode { get; private set; }
			public bool NoFog { get; set; }
			public float TotalElapsedTimeInSeconds => (float)_elapsedTimer.Elapsed.TotalSeconds;
			public Vector4 GBufferOffsets { get; private set; }
			public RenderTarget2D DepthBuffer { get; set; }
			public RenderTarget2D ScreenTexture { get; set; }
			public RenderTarget2D ReflectionTexture { get; set; }

			public Color EnvColor
			{
				get
				{
					if (Environment.FogEnabled)
					{
						return Environment.FogColor;
					}

					if (Environment.Sky != null && Environment.Sky.Visible)
					{
						return Environment.Sky.DiffuseColor;
					}

					return Color.White;

				}
			}

			public TextureCube EnvCubeMap
			{
				get
				{
					if (Environment.Sky != null && Environment.Sky.Visible && Environment.Sky.DiffuseTexture != null)
					{
						return Environment.Sky.DiffuseTexture;
					}

					return Resources.WhiteCube;
				}
			}



			public EffectLevelBindingContext(ForwardRenderer renderer)
			{
				Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
				_elapsedTimer.Reset();
				_elapsedTimer.Start();
			}

			public void SetLight(BaseLight light)
			{
				if (light == null)
				{
					LightColor = Color.Transparent;
					return;
				}

				LightColor = light.Color;

				var asDirectLight = light as DirectLight;
				if (asDirectLight != null)
				{
					LightDir = -asDirectLight.Direction;
					DirectLight = asDirectLight;

					ShadowParams = new Vector3(DirectLight.InternalShadowBase, DirectLight.InternalShadowIntensity, DirectLight.InternalShadowBias);

					var manager = DirectLight.ShadowCascadeManager;
					NormalizedShadowDistances = GetDistancesAsVector(manager, _camera.FarPlane);

					ShadowDepthFade = CalculateDepthFade(manager.MaxDistance, DirectLight.InternalShadowFadeStart, _camera.FarPlane);
				}

				var asPointLight = light as PointLight;
				if (asPointLight != null)
				{
					LightPos = new Vector4(asPointLight.GlobalTransform.Translation, 1.0f / asPointLight.Range);
					PointLightRamp = asPointLight.Ramp;
				}

				var asSpotLight = light as SpotLight;
				if (asSpotLight != null)
				{
					LightPos = new Vector4(asSpotLight.GlobalTransform.Translation, 1.0f / asSpotLight.Range);
					SpotLightMatrix = asSpotLight.SpotMatrix;
					SpotLightRamp = asSpotLight.Ramp;
				}
			}

			private void UpdateEnvironment()
			{
				if (_camera == null || _environment == null)
				{
					return;
				}

				float fogStart, fogEnd;
				if (!_environment.FogEnabled)
				{
					fogStart = _camera.FarPlane;
					fogEnd = fogStart * 2;
				}
				else
				{
					fogStart = (float)Math.Min(_environment.FogStart, _camera.FarPlane);
					fogEnd = (float)Math.Min(_environment.FogEnd, _camera.FarPlane);
				}

				var fogRange = fogEnd - fogStart;
				FogParams = new Vector4(fogEnd / _camera.FarPlane, _camera.FarPlane / fogRange, 0, 0);
			}

			public void SetGBufferOffsets(Point texSize, Rectangle viewRect)
			{
				var texWidth = (float)texSize.X;
				var texHeight = (float)texSize.Y;
				var widthRange = 0.5f * viewRect.Width / texWidth;
				var heightRange = 0.5f * viewRect.Height / texHeight;

				var result = new Vector4(((float)viewRect.Left) / texWidth + widthRange,
					1.0f - (((float)viewRect.Top) / texHeight + heightRange),
					widthRange,
					heightRange);

				GBufferOffsets = result;
			}

			public void Reset()
			{
				Camera = null;
				Environment = null;
				DirectLight = null;
				CSMData = null;
			}

			private Vector4 GetDistancesAsVector(ShadowCascadeManager manager, float far)
			{
				var shadowSplits = new Vector4(manager.GetSplitDistance(0) / far, 1.0f, 1.0f, 1.0f);

				if (manager.Cascades > 1)
				{
					shadowSplits.Y = manager.GetSplitDistance(1) / far;
				}

				if (manager.Cascades > 2)
				{
					shadowSplits.Z = manager.GetSplitDistance(2) / far;
				}

				return shadowSplits;
			}

			private Vector4 CalculateDepthFade(float maxShadowDistance,
				float shadowFadeStart,
				float far)
			{
				var result = new Vector4();

				var shadowRange = maxShadowDistance;
				var fadeStart = shadowFadeStart * shadowRange / far;
				var fadeEnd = shadowRange / far;
				var fadeRange = fadeEnd - fadeStart;

				result.Z = fadeStart;
				result.W = 1.0f / fadeRange;

				return result;
			}
		}

		private readonly EffectLevelBindingContext _bindingContext;

		private static readonly Action<EffectLevelBindingContext, EffectParameter>[] _effectLevelParameterSetters = new Action<EffectLevelBindingContext, EffectParameter>[Enum.GetValues(typeof(EffectLevelParameter)).Length];
		private static readonly Action<RenderJob, EffectParameter>[] _meshPartLevelParameterSetters = new Action<RenderJob, EffectParameter>[Enum.GetValues(typeof(MeshPartLevelParameter)).Length];

		private static void SetEffectLevelParameterSetters()
		{
			SetEffectLevelParameterSetter(EffectLevelParameter.ViewProj, (ctx, p) => p.SetValue(ctx.Camera.ViewProjection));
			SetEffectLevelParameterSetter(EffectLevelParameter.CameraPos, (ctx, p) => p.SetValue(ctx.Camera.Translation));
			SetEffectLevelParameterSetter(EffectLevelParameter.NearPlane, (ctx, p) => p.SetValue(ctx.Camera.NearPlane));
			SetEffectLevelParameterSetter(EffectLevelParameter.FarPlane, (ctx, p) => p.SetValue(ctx.Camera.FarPlane));
			SetEffectLevelParameterSetter(EffectLevelParameter.InverseView, (ctx, p) => p.SetValue(ctx.Camera.InverseView));
			SetEffectLevelParameterSetter(EffectLevelParameter.LightMatrices, (ctx, p) => p.SetValue(ctx.CSMData.LightViewProjs));
			SetEffectLevelParameterSetter(EffectLevelParameter.ShadowSplits, (ctx, p) => p.SetValue(ctx.NormalizedShadowDistances));
			SetEffectLevelParameterSetter(EffectLevelParameter.ShadowMap, (ctx, p) => p.SetValue(ctx.CSMData.ShadowMap));
			SetEffectLevelParameterSetter(EffectLevelParameter.ShadowParams, (ctx, p) => p.SetValue(ctx.ShadowParams));
			SetEffectLevelParameterSetter(EffectLevelParameter.ShadowMapInvSize, (ctx, p) => p.SetValue(EffectLevelBindingContext.ShadowMapInverseSize));
			SetEffectLevelParameterSetter(EffectLevelParameter.ShadowDepthFade, (ctx, p) => p.SetValue(ctx.ShadowDepthFade));
			SetEffectLevelParameterSetter(EffectLevelParameter.DepthMode, (ctx, p) => p.SetValue(ctx.DepthMode));
			SetEffectLevelParameterSetter(EffectLevelParameter.LightColor, (ctx, p) => p.SetValue(ctx.LightColor.ToVector4()));
			SetEffectLevelParameterSetter(EffectLevelParameter.LightDir, (ctx, p) => p.SetValue(ctx.LightDir));
			SetEffectLevelParameterSetter(EffectLevelParameter.LightPos, (ctx, p) => p.SetValue(ctx.LightPos));
			SetEffectLevelParameterSetter(EffectLevelParameter.LightRampMap, (ctx, p) => p.SetValue(Resources.GetPointLightRamp(ctx.PointLightRamp)));
			SetEffectLevelParameterSetter(EffectLevelParameter.LightSpotMap, (ctx, p) => p.SetValue(Resources.GetSpotLightRamp(ctx.SpotLightRamp)));
			SetEffectLevelParameterSetter(EffectLevelParameter.SpotLightMatrix, (ctx, p) => p.SetValue(ctx.SpotLightMatrix));
			SetEffectLevelParameterSetter(EffectLevelParameter.UOffset, (ctx, p) => p.SetValue(new Vector4(1, 0, 0, 0)));
			SetEffectLevelParameterSetter(EffectLevelParameter.VOffset, (ctx, p) => p.SetValue(new Vector4(0, 1, 0, 0)));

			SetEffectLevelParameterSetter(EffectLevelParameter.AmbientLightColor, (ctx, p) =>
			{
				var color = ctx.NoFog ? Color.Transparent : ctx.Environment.AmbientLightColor;
				p.SetValue(color.ToVector3());
			});

			SetEffectLevelParameterSetter(EffectLevelParameter.FogColor, (ctx, p) =>
			{
				var color = ctx.NoFog ? Color.Transparent : ctx.Environment.FogColor;
				p.SetValue(color.ToVector3());
			});

			SetEffectLevelParameterSetter(EffectLevelParameter.FogParams, (ctx, p) => p.SetValue(ctx.FogParams));
			SetEffectLevelParameterSetter(EffectLevelParameter.ElapsedTime, (ctx, p) => p.SetValue(ctx.TotalElapsedTimeInSeconds));
			SetEffectLevelParameterSetter(EffectLevelParameter.GBufferOffsets, (ctx, p) => p.SetValue(ctx.GBufferOffsets));
			SetEffectLevelParameterSetter(EffectLevelParameter.ScreenMap, (ctx, p) => p.SetValue(ctx.ScreenTexture));
			SetEffectLevelParameterSetter(EffectLevelParameter.DepthMap, (ctx, p) => p.SetValue(ctx.DepthBuffer));
			SetEffectLevelParameterSetter(EffectLevelParameter.EnvColor, (ctx, p) => p.SetValue(ctx.EnvColor.ToVector4()));
			SetEffectLevelParameterSetter(EffectLevelParameter.EnvCubeMap, (ctx, p) => p.SetValue(ctx.EnvCubeMap));
		}

		private static void SetMeshPartLevelParameterSetters()
		{
			SetMeshPartLevelParameterSetter(MeshPartLevelParameter.Model, (j, p) => p.SetValue(j.Transform));
			SetMeshPartLevelParameterSetter(MeshPartLevelParameter.ModelViewProj, (j, p) => p.SetValue(j.ModelViewProj));
			SetMeshPartLevelParameterSetter(MeshPartLevelParameter.SkinMatrices, (j, p) => p.SetValue(j.BonesTransforms));
			SetMeshPartLevelParameterSetter(MeshPartLevelParameter.ReflectionMap, (j, p) => p.SetValue(j.ReflectionTexture));
			SetMeshPartLevelParameterSetter(MeshPartLevelParameter.ClipPlane, (j, p) => p.SetValue(j.ClipPlane.Value.ToVector4()));
		}

		private static void SetEffectLevelParameterSetter(EffectLevelParameter p, Action<EffectLevelBindingContext, EffectParameter> setter)
		{
			_effectLevelParameterSetters[(int)p] = setter;
		}

		private static void SetMeshPartLevelParameterSetter(MeshPartLevelParameter p, Action<RenderJob, EffectParameter> setter)
		{
			_meshPartLevelParameterSetters[(int)p] = setter;
		}
	}
}