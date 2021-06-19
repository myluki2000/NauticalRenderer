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
float LineAndGapLengths[4];
float4 BackgroundColor;

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
    float segmentLength = LineAndGapLengths[0] + LineAndGapLengths[1] + LineAndGapLengths[2] + LineAndGapLengths[3];
    float distMod = length(input.VertexPosition - input.Position.xy) % segmentLength;
    bool hasLine =  distMod < LineAndGapLengths[0]
					|| ((distMod > (LineAndGapLengths[0] + LineAndGapLengths[1]) && (distMod < segmentLength - LineAndGapLengths[3])));
	
    float4 color = float4(input.Color.r * hasLine + BackgroundColor.r * !hasLine,
						  input.Color.g * hasLine + BackgroundColor.g * !hasLine,
						  input.Color.b * hasLine + BackgroundColor.b * !hasLine,
						  input.Color.a * hasLine + BackgroundColor.a * !hasLine);
	
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