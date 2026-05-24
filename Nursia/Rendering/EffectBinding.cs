using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using System;
using System.Collections.Generic;

namespace Nursia.Rendering
{
	/// <summary>
	/// Enumerates effect-level parameters that can be set on shader effects.
	/// </summary>
	public enum EffectLevelParameter
	{
		/// <summary>
		/// View-projection matrix parameter.
		/// </summary>
		ViewProj,
		/// <summary>
		/// Inverse view matrix parameter.
		/// </summary>
		InverseView,
		/// <summary>
		/// Camera position parameter.
		/// </summary>
		CameraPos,
		/// <summary>
		/// Near plane distance parameter.
		/// </summary>
		NearPlane,
		/// <summary>
		/// Far plane distance parameter.
		/// </summary>
		FarPlane,
		/// <summary>
		/// Texture U-offset parameter.
		/// </summary>
		UOffset,
		/// <summary>
		/// Texture V-offset parameter.
		/// </summary>
		VOffset,
		/// <summary>
		/// Depth mode parameter.
		/// </summary>
		DepthMode,
		/// <summary>
		/// Ambient light color parameter.
		/// </summary>
		AmbientLightColor,
		/// <summary>
		/// Light color parameter.
		/// </summary>
		LightColor,
		/// <summary>
		/// Light direction parameter.
		/// </summary>
		LightDir,
		/// <summary>
		/// Light position parameter.
		/// </summary>
		LightPos,
		/// <summary>
		/// Light ramp map parameter.
		/// </summary>
		LightRampMap,
		/// <summary>
		/// Light spot map parameter.
		/// </summary>
		LightSpotMap,
		/// <summary>
		/// Spot light transformation matrix parameter.
		/// </summary>
		SpotLightMatrix,
		/// <summary>
		/// Light transformation matrices parameter.
		/// </summary>
		LightMatrices,
		/// <summary>
		/// Shadow split distances parameter.
		/// </summary>
		ShadowSplits,
		/// <summary>
		/// Shadow map texture parameter.
		/// </summary>
		ShadowMap,
		/// <summary>
		/// Shadow parameters parameter.
		/// </summary>
		ShadowParams,
		/// <summary>
		/// Shadow map inverse size parameter.
		/// </summary>
		ShadowMapInvSize,
		/// <summary>
		/// Shadow depth fade parameter.
		/// </summary>
		ShadowDepthFade,
		/// <summary>
		/// Fog parameters parameter.
		/// </summary>
		FogParams,
		/// <summary>
		/// Fog color parameter.
		/// </summary>
		FogColor,
		/// <summary>
		/// Elapsed time parameter.
		/// </summary>
		ElapsedTime,
		/// <summary>
		/// G-buffer offsets parameter.
		/// </summary>
		GBufferOffsets,
		/// <summary>
		/// Screen map parameter.
		/// </summary>
		ScreenMap,
		/// <summary>
		/// Depth map parameter.
		/// </summary>
		DepthMap,
		/// <summary>
		/// Environment color parameter.
		/// </summary>
		EnvColor,
		/// <summary>
		/// Environment cube map parameter.
		/// </summary>
		EnvCubeMap
	}

	/// <summary>
	/// Enumerates mesh-part-level parameters that can be set on shader effects.
	/// </summary>
	public enum MeshPartLevelParameter
	{
		/// <summary>
		/// Model transformation matrix parameter.
		/// </summary>
		Model,
		/// <summary>
		/// Model-view-projection matrix parameter.
		/// </summary>
		ModelViewProj,
		/// <summary>
		/// Skin transformation matrices parameter.
		/// </summary>
		SkinMatrices,
		/// <summary>
		/// Reflection map texture parameter.
		/// </summary>
		ReflectionMap,
		/// <summary>
		/// Clip plane parameter.
		/// </summary>
		ClipPlane
	}

	/// <summary>
	/// Represents an effect parameter that is set at the material level.
	/// </summary>
	public class MaterialLevelEffectParameter
	{
		/// <summary>
		/// Gets the name of the effect parameter.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// Gets the underlying effect parameter.
		/// </summary>
		public EffectParameter Parameter { get; }
		/// <summary>
		/// Gets the setter action that applies the material's value to the parameter.
		/// </summary>
		public Action<object, EffectParameter> Setter { get; }

		internal MaterialLevelEffectParameter(string name, EffectParameter parameter, Action<object, EffectParameter> setter)
		{
			Name = name;
			Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
			Setter = setter ?? throw new ArgumentNullException(nameof(setter));
		}
	}

	/// <summary>
	/// Manages shader effect parameters and their bindings for rendering operations.
	/// </summary>
	public partial class EffectBinding
	{
		private static int _lastBatchId = 0;
		private Effect _effect;
		private readonly Dictionary<EffectLevelParameter, EffectParameter> _effectParameters = new Dictionary<EffectLevelParameter, EffectParameter>();
		private readonly Dictionary<MeshPartLevelParameter, EffectParameter> _meshParameters = new Dictionary<MeshPartLevelParameter, EffectParameter>();
		private readonly Dictionary<string, MaterialLevelEffectParameter> _materialLevelSetters = new Dictionary<string, MaterialLevelEffectParameter>();

		/// <summary>
		/// Gets the unique batch identifier for this effect binding.
		/// </summary>
		public int BatchId { get; }

		/// <summary>
		/// Gets a read-only dictionary of effect-level parameters bound to this effect.
		/// </summary>
		public IReadOnlyDictionary<EffectLevelParameter, EffectParameter> EffectLevelParameters => _effectParameters;
		/// <summary>
		/// Gets a read-only dictionary of mesh-part-level parameters bound to this effect.
		/// </summary>
		public IReadOnlyDictionary<MeshPartLevelParameter, EffectParameter> MeshPartLevelParameters => _meshParameters;
		/// <summary>
		/// Gets a read-only dictionary of material-level parameter setters.
		/// </summary>
		public IReadOnlyDictionary<string, MaterialLevelEffectParameter> MaterialLevelSetters => _materialLevelSetters;

		/// <summary>
		/// Gets or sets the shader effect for this binding.
		/// </summary>
		/// <remarks>
		/// When setting a new effect, its parameters are automatically bound to this effect binding.
		/// If the current effect becomes invalid, it will be automatically updated.
		/// </remarks>
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

		/// <summary>
		/// Gets the shader technique used for rendering.
		/// </summary>
		public EffectTechnique Technique { get; }

		/// <summary>
		/// Initializes a new instance of the EffectBinding class.
		/// </summary>
		public EffectBinding()
		{
			BatchId = _lastBatchId;
			++_lastBatchId;
		}

		/// <summary>
		/// Initializes a new instance of the EffectBinding class with a specific effect and optional technique.
		/// </summary>
		/// <param name="effect">The shader effect to bind.</param>
		/// <param name="technique">The name of the technique to use, or null to use the default technique.</param>
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

		/// <summary>
		/// Adds a material-level parameter setter for the specified parameter name.
		/// </summary>
		/// <typeparam name="T">The material type that will provide values for the parameter.</typeparam>
		/// <param name="parameterName">The name of the effect parameter.</param>
		/// <param name="setter">The action that sets the parameter value from the material.</param>
		/// <returns>True if the setter was successfully added; false if the parameter does not exist in the effect.</returns>
		public bool AddMaterialLevelSetter<T>(string parameterName, Action<T, EffectParameter> setter) where T : IMaterial
		{
			return InternalAddMaterialLevelSetter(parameterName, (o, p) => setter((T)o, p));
		}
	}
}
