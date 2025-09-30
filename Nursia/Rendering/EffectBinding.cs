using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	public enum EffectLevelParameter
	{
		ViewProj,
		InverseView,
		CameraPos,
		NearPlane,
		FarPlane,
		UOffset,
		VOffset,
		DepthMode,
		AmbientLightColor,
		LightColor,
		LightDir,
		LightPos,
		LightRampMap,
		LightSpotMap,
		SpotLightMatrix,
		LightMatrices,
		ShadowSplits,
		ShadowMap,
		ShadowParams,
		ShadowMapInvSize,
		ShadowDepthFade,
		FogParams,
		FogColor,
		ElapsedTime,
		GBufferOffsets,
		ScreenMap,
		DepthMap,
		EnvColor,
		EnvCubeMap
	}

	public enum MeshPartLevelParameter
	{
		Model,
		ModelViewProj,
		SkinMatrices,
		ReflectionMap,
		ClipPlane
	}

	public class MaterialLevelEffectParameter
	{
		public string Name { get; }
		public EffectParameter Parameter { get; }
		public Action<object, EffectParameter> Setter { get; }

		internal MaterialLevelEffectParameter(string name, EffectParameter parameter, Action<object, EffectParameter> setter)
		{
			Name = name;
			Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
			Setter = setter ?? throw new ArgumentNullException(nameof(setter));
		}
	}

	public partial class EffectBinding
	{
		private static int _lastBatchId = 0;
		private Effect _effect;
		private readonly Dictionary<EffectLevelParameter, EffectParameter> _effectParameters = new Dictionary<EffectLevelParameter, EffectParameter>();
		private readonly Dictionary<MeshPartLevelParameter, EffectParameter> _meshParameters = new Dictionary<MeshPartLevelParameter, EffectParameter>();
		private readonly Dictionary<string, MaterialLevelEffectParameter> _materialLevelSetters = new Dictionary<string, MaterialLevelEffectParameter>();

		public int BatchId { get; }

		public IReadOnlyDictionary<EffectLevelParameter, EffectParameter> EffectLevelParameters => _effectParameters;
		public IReadOnlyDictionary<MeshPartLevelParameter, EffectParameter> MeshPartLevelParameters => _meshParameters;
		public IReadOnlyDictionary<string, MaterialLevelEffectParameter> MaterialLevelSetters => _materialLevelSetters;

		public Effect Effect
		{
			get
			{
				if (_effect != null)
				{
					if (!Nrs.EffectsSource.IsEffectValid(_effect))
					{
						var oldEffect = _effect;
						_effect = Nrs.EffectsSource.UpdateEffect(_effect);
						BindParameters();

						if (_effect != oldEffect)
						{
							oldEffect.Dispose();
						}
					}
				}

				return _effect;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				if (value == _effect)
				{
					return;
				}

				_effect = value;
				BindParameters();
			}
		}

		public EffectTechnique Technique { get; }

		public EffectBinding()
		{
			BatchId = _lastBatchId;
			++_lastBatchId;
		}

		public EffectBinding(Effect effect, string technique = null): this()
		{
			Effect = effect;

			if (!string.IsNullOrEmpty(technique))
			{
				Technique = effect.Techniques[technique];
			} else
			{
				Technique = null;
			}
		}

		protected virtual void BindParameters()
		{
			_meshParameters.Clear();
			_effectParameters.Clear();

			foreach (var param in Effect.Parameters)
			{
				ParameterInfo info;
				if (!_allParameters.TryGetValue(param.Name, out info))
				{
					continue;
				}

				switch (info.Usage)
				{
					case EffectParameterLevel.MeshPart:
						_meshParameters[(MeshPartLevelParameter)info.Parameter] = param;
						break;

					default:
						_effectParameters[(EffectLevelParameter)info.Parameter] = param;
						break;
				}
			}

			if (_materialLevelSetters.Count > 0)
			{
				// Save old setters
				var oldSetters = new Dictionary<string, MaterialLevelEffectParameter>();
				foreach (var pair in _materialLevelSetters)
				{
					oldSetters[pair.Key] = pair.Value;
				}

				// Clear and re-add setters
				_materialLevelSetters.Clear();

				foreach (var pair in oldSetters)
				{
					InternalAddMaterialLevelSetter(pair.Key, pair.Value.Setter);
				}
			}
		}

		private bool InternalAddMaterialLevelSetter(string parameterName, Action<object, EffectParameter> setter)
		{
			var parameter = Effect.Parameters[parameterName];
			if (parameter == null)
			{
				return false;
			}

			_materialLevelSetters[parameterName] = new MaterialLevelEffectParameter(parameterName, parameter, setter);

			return true;
		}

		public bool AddMaterialLevelSetter<T>(string parameterName, Action<T, EffectParameter> setter) where T : IMaterial
		{
			return InternalAddMaterialLevelSetter(parameterName, (o, p) => setter((T)o, p));
		}
	}
}
