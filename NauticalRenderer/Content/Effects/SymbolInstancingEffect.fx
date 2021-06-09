#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "Macros.fxh"

DECLARE_TEXTURE(Texture, 0);

matrix WorldMatrix;
matrix ViewportMatrix;
float Size;
int AtlasWidth;


struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input, float2 instanceTransform : POSITION1, float2 atlasCoord : TEXCOORD1)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(round(mul(float4(instanceTransform.x, instanceTransform.y, 0, 1), WorldMatrix) + float4(input.Position * Size, 0)), ViewportMatrix);
    output.TexCoord = atlasCoord + input.TexCoord / AtlasWidth;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return SAMPLE_TEXTURE(Texture, input.TexCoord);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};