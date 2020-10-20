//-----------------------------------------------------------------------------
// Macros.fxh
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { AlphaBlendEnable = TRUE; DestBlend = INVSRCALPHA; SrcBlend = SRCALPHA; VertexShader = compile vs_4_0 vsname (); PixelShader = compile ps_4_0 psname(); } }

#define BEGIN_CONSTANTS     cbuffer Parameters : register(b0) {
#define MATRIX_CONSTANTS
#define END_CONSTANTS       };

#define DECLARE_TEXTURE(Name, index) \
    Texture2D<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index)

#define DECLARE_CUBEMAP(Name, index) \
    TextureCube<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)

