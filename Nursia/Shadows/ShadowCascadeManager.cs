using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Lights;
using Nursia.Utilities;
using System;

namespace Nursia.Shadows
{
	internal class ShadowCascadeManager
	{
		private const float ShadowMapCascadeSplitLambda = 0.8f;
		private const float MinimumDistance = 0.5f;

		private int _cascades = 4;
		private float[] _splits = new float[4] { 0.1f, 0.2f, 0.5f, 1.0f };
		private float _maxDistance = 200.0f;

		public int Cascades
		{
			get => _cascades;

			set
			{
				if (value < 1 || value > 4)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_cascades = value;
			}
		}

		public float Split1
		{
			get => _splits[0];

			set
			{
				if (value < MinimumDistance / 100.0f || value > 1.0f)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_splits[0] = value;
			}
		}

		public float Split2
		{
			get => _splits[1];

			set
			{
				if (value < MinimumDistance / 100.0f || value > 1.0f)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_splits[1] = value;
			}
		}

		public float Split3
		{
			get => _splits[2];

			set
			{
				if (value < MinimumDistance / 100.0f || value > 1.0f)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_splits[2] = value;
			}
		}

		public float MaxDistance
		{
			get => _maxDistance;

			set
			{
				if (value < MinimumDistance)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_maxDistance = value;
			}
		}

		private int Cols
		{
			get
			{
				return Cascades == 1 ? 1 : 2;
			}
		}

		private int Rows
		{
			get
			{
				return Cascades < 3 ? 1 : 2;
			}
		}

		public float GetSplit(int index)
		{
			return index >= Cascades - 1?1.0f:_splits[index];
		}

		public float GetSplitDistance(int index) => GetSplit(index) * MaxDistance;

		public void UpdateShadowMapParameters(Camera camera, Vector3 lightDirection, DirectLightCSMData output)
		{
			output.CascadesCount = Cascades;
			var size = Nrs.GraphicsSettings.ShadowMapSize.GetSize();
			var nearPlane = camera.NearPlane;
			for (var i = 0; i < Cascades; ++i)
			{
				var farPlane = GetSplitDistance(i);

				var sourceCamera = output.SourceCameras[i];
				sourceCamera.CopyViewParams(camera);

				sourceCamera.NearPlane = nearPlane;
				sourceCamera.FarPlane = farPlane;

				Mathematics.UpdateLightCamera(output.SourceCameras[i].Frustum, lightDirection, output.Cameras[i]);

				output.LightViewProjs[i] = output.Cameras[i].ViewProjection;

				// Calculate texture transform matrix
				var col = i % 2;
				var row = i / 2;
				var offset = new Vector3((float)col / Cols, (float)row / Rows, 0);
				var scale = new Vector3(0.5f / Cols, 0.5f / Rows, 1.0f);

				offset.X += scale.X + 0.5f / size;
				offset.Y += scale.Y + 0.5f / size;

				scale.Y = -scale.Y;

				var texAdjust = Matrix.Identity;
				texAdjust.Translation = offset;
				texAdjust.M11 = scale.X;
				texAdjust.M22 = scale.Y;
				texAdjust.M33 = scale.Z;

				if (Nrs.GraphicsSettings.ShadowType == ShadowType.PCF)
				{
					offset.X -= 0.5f / size;
					offset.Y -= 0.5f / size;
				}

				output.LightViewProjs[i] = output.LightViewProjs[i] * texAdjust;

				// Move to the next cascade
				nearPlane = farPlane;
			}
		}

		public Viewport GetCascadeViewport(int cascadeIndex)
		{
			var cascadesCount = Cascades;
			var size = Nrs.GraphicsSettings.ShadowMapSize.GetSize();
			if (cascadesCount == 1)
			{
				// One cascade occupying whole texture
				return new Viewport(0, 0, size, size);
			}
			else if (cascadesCount == 2)
			{
				// Two cascades: left and right
				if (cascadeIndex == 0)
				{
					return new Viewport(0, 0, size / 2, size);
				}

				return new Viewport(size / 2, 0, size / 2, size);
			}

			// Standart partition, where texture is split into four cascades
			var col = cascadeIndex % 2;
			var row = cascadeIndex / 2;

			var cascadeSize = Nrs.GraphicsSettings.ShadowMapSize.GetSize() / 2;

			return new Viewport(col * cascadeSize, row * cascadeSize, cascadeSize, cascadeSize);
		}

		public void MeasureDistances(float near, float far, int cascades)
		{
			// Calculate split depths based on view camera frustum
			// Based on method presented in https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch10.html
			var ratio = far / near;
			var range = far - near;

			Cascades = cascades;
			MaxDistance = far;

			for (var i = 0; i < cascades; ++i)
			{
				var p = (float)(i + 1) / cascades;
				var log = near * (float)Math.Pow(ratio, p);
				var uniform = near + range * p;
				var d = ShadowMapCascadeSplitLambda * (log - uniform) + uniform;

				_splits[i] = (d - near) / MaxDistance;
			}
		}
	}
}
