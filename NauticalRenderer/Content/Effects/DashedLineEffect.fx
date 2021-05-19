#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

matrix WorldMatrix;
matrix ViewportMatrix;
float lineAndGapLengths[4];

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
    nointerpolation float2 VertexPosition : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, mul(WorldMatrix, ViewportMatrix));
    output.VertexPosition = mul(input.Position, WorldMatrix);
	output.Color = input.Color;
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float segmentLength = lineAndGapLengths[0] + lineAndGapLengths[1] + lineAndGapLengths[2] + lineAndGapLengths[3];
    float distMod = length(input.VertexPosition - input.Position.xy) % segmentLength;
    bool hasLine =  distMod < lineAndGapLengths[0]
					|| ((distMod > (lineAndGapLengths[0] + lineAndGapLengths[1]) && (distMod < segmentLength - lineAndGapLengths[3])));
	
    float4 color = float4(input.Color.r * hasLine,
						  input.Color.g * hasLine,
						  input.Color.b * hasLine,
						  hasLine);
	
    return color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};