using System.Collections.Generic;

namespace Nursia.Rendering
{
	partial class EffectBinding
	{
		private enum EffectParameterLevel
		{
			Effect,
			MeshPart
		}

		private class ParameterInfo
		{
			public int Parameter { get; }
			public EffectParameterLevel Usage { get; }

			public ParameterInfo(EffectLevelParameter parameter)
			{
				Parameter = (int)parameter;
				Usage = EffectParameterLevel.Effect;
			}

			public ParameterInfo(MeshPartLevelParameter parameter)
			{
				Parameter = (int)parameter;
				Usage = EffectParameterLevel.MeshPart;
			}
		}

		private static readonly Dictionary<string, ParameterInfo> _allParameters = new Dictionary<string, ParameterInfo>();

		static EffectBinding()
		{
			_allParameters["cViewProj"] = new ParameterInfo(EffectLevelParameter.ViewProj);
			_allParameters["cInverseView"] = new ParameterInfo(EffectLevelParameter.InverseView);
			_allParameters["cCameraPos"] = new ParameterInfo(EffectLevelParameter.CameraPos);
			_allParameters["cNearPlane"] = new ParameterInfo(EffectLevelParameter.NearPlane);
			_allParameters["cFarPlane"] = new ParameterInfo(EffectLevelParameter.FarPlane);
			_allParameters["cUOffset"] = new ParameterInfo(EffectLevelParameter.UOffset);
			_allParameters["cVOffset"] = new ParameterInfo(EffectLevelParameter.VOffset);
			_allParameters["cDepthMode"] = new ParameterInfo(EffectLevelParameter.DepthMode);

			_allParameters["cAmbientLightColor"] = new ParameterInfo(EffectLevelParameter.AmbientLightColor);
			_allParameters["cLightColor"] = new ParameterInfo(EffectLevelParameter.LightColor);
			_allParameters["cLightDir"] = new ParameterInfo(EffectLevelParameter.LightDir);

			_allParameters["cLightPos"] = new ParameterInfo(EffectLevelParameter.LightPos);
			_allParameters["LightRampMap"] = new ParameterInfo(EffectLevelParameter.LightRampMap);

			_allParameters["cSpotLightMatrix"] = new ParameterInfo(EffectLevelParameter.SpotLightMatrix);
			_allParameters["LightSpotMap"] = new ParameterInfo(EffectLevelParameter.LightSpotMap);

			_allParameters["cLightMatrices"] = new ParameterInfo(EffectLevelParameter.LightMatrices);
			_allParameters["cShadowSplits"] = new ParameterInfo(EffectLevelParameter.ShadowSplits);
			_allParameters["ShadowMap"] = new ParameterInfo(EffectLevelParameter.ShadowMap);
			_allParameters["cShadowParams"] = new ParameterInfo(EffectLevelParameter.ShadowParams);
			_allParameters["cShadowMapInvSize"] = new ParameterInfo(EffectLevelParameter.ShadowMapInvSize);
			_allParameters["cShadowDepthFade"] = new ParameterInfo(EffectLevelParameter.ShadowDepthFade);

			_allParameters["cFogParams"] = new ParameterInfo(EffectLevelParameter.FogParams);
			_allParameters["cFogColor"] = new ParameterInfo(EffectLevelParameter.FogColor);

			_allParameters["cElapsedTime"] = new ParameterInfo(EffectLevelParameter.ElapsedTime);
			_allParameters["cGBufferOffsets"] = new ParameterInfo(EffectLevelParameter.GBufferOffsets);
			_allParameters["ScreenMap"] = new ParameterInfo(EffectLevelParameter.ScreenMap);
			_allParameters["DepthMap"] = new ParameterInfo(EffectLevelParameter.DepthMap);
			_allParameters["cEnvColor"] = new ParameterInfo(EffectLevelParameter.EnvColor);
			_allParameters["EnvCubeMap"] = new ParameterInfo(EffectLevelParameter.EnvCubeMap);

			_allParameters["ReflectionMap"] = new ParameterInfo(MeshPartLevelParameter.ReflectionMap);
			_allParameters["cModel"] = new ParameterInfo(MeshPartLevelParameter.Model);
			_allParameters["cModelViewProj"] = new ParameterInfo(MeshPartLevelParameter.ModelViewProj);
			_allParameters["cSkinMatrices"] = new ParameterInfo(MeshPartLevelParameter.SkinMatrices);
			_allParameters["cClipPlane"] = new ParameterInfo(MeshPartLevelParameter.ClipPlane);
		}
	}
}
