#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#include "Macros.fxh"

DECLARE_TEXTURE(Texture, 0);

matrix WorldMatrix;
matrix ViewportMatrix;
float Size;

struct VSInput
{
	float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
	
};

VSOutput MainVS(in VSInput input, float2 instanceTransform : POSITION1, int buoyShape : TEXCOORD1, int colorPattern : TEXCOORD2, float4 colors[4] : COLOR0)
{
	VSOutput output = (VSOutput)0;
	output.Position = mul(mul(float4(instanceTransform.x, instanceTransform.y, 0, 1), WorldMatrix) + float4(input.Position * Size, 0), ViewportMatrix);

	return output;
}

float4 MainPS(VSOutput input) : COLOR
{
	

    return float4(0, 0, 0, 1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};