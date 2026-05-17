//-----------------------------------------------------------------------------
// Macros.fxh
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#ifdef SM4

// Macros for targetting shader model 4.0 (DX11)

#define VS_PROFILE vs_4_0
#define PS_PROFILE ps_4_0

#else

// Macros for targetting shader model 3.0 (mojoshader)

#define VS_PROFILE vs_3_0
#define PS_PROFILE ps_3_0

#endif

#define DECLARE_TEXTURE2D_LINEAR_CLAMP(Name) \
	texture2D Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU = Clamp; AddressV = Clamp; };

#define DECLARE_TEXTURECUBE_LINEAR_CLAMP(Name) \
	textureCUBE Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU = Clamp; AddressV = Clamp; };

#define DECLARE_TEXTURE2D_LINEAR_WRAP(Name) \
	texture2D Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU = Wrap; AddressV = Wrap; };

#define DECLARE_TEXTURECUBE_LINEAR_WRAP(Name) \
	textureCUBE Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU = Wrap; AddressV = Wrap; };
	
#define Sample2D(tex, uv) tex2D(s##tex, uv)
#define SampleCube(tex, uv) texCUBE(s##tex, uv)

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile VS_PROFILE vsname (); PixelShader = compile PS_PROFILE psname(); } }
