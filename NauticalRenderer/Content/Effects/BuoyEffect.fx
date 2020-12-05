#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#include "Macros.fxh"

#define BUOY_SHAPE_CONICAL		0
#define BUOY_SHAPE_CAN			1
#define BUOY_SHAPE_SPHERICAL	2
#define BUOY_SHAPE_PILLAR		3
#define BUOY_SHAPE_SPAR			4
#define BUOY_SHAPE_BARREL		5
#define BUOY_SHAPE_SUPER_BUOY	6
#define BUOY_SHAPE_ICE_BUOY		7

#define COLOR_PATTERN_NONE			0
#define COLOR_PATTERN_HORIZONTAL	1
#define COLOR_PATTERN_VERTICAL		2
#define COLOR_PATTERN_DIAGONAL		3
#define COLOR_PATTERN_SQUARED		4
#define COLOR_PATTERN_STRIPES		5
#define COLOR_PATTERN_BORDER		6
#define COLOR_PATTERN_CROSS			7
#define COLOR_PATTERN_SALTIRE		8


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
	int BuoyShape : TEXCOORD1;
	int ColorPattern : TEXCOORD2;
	float4 Colors[4] : COLOR0;
};

VSOutput MainVS(in VSInput input, float2 instanceTransform : POSITION1, int buoyShape : TEXCOORD1, int colorPattern : TEXCOORD2, float4 colors[4] : COLOR0)
{
	VSOutput output = (VSOutput)0;
    output.Position = mul(round(mul(float4(instanceTransform.x, instanceTransform.y, 0, 1), WorldMatrix) + float4(input.Position * Size, 0)), ViewportMatrix);
	output.Colors = colors;
	output.ColorPattern = colorPattern;
	output.BuoyShape = buoyShape;
	output.TexCoord = input.TexCoord;

	return output;
}

float4 MainPS(VSOutput input) : COLOR
{
	const float ORIGIN_FRAC = 460.0f / 512.0f;

	int colorCount = 0;

    colorCount += input.Colors[0].a > 0.0f;
    colorCount += input.Colors[1].a > 0.0f;
    colorCount += input.Colors[2].a > 0.0f;
    colorCount += input.Colors[3].a > 0.0f;


	float colorAngle = 0.0f;
	float2 atlasCoord = float2(0.0f, 0.0f);
	
    if (input.BuoyShape == BUOY_SHAPE_CONICAL)
    {
        atlasCoord = float2(0.0f, 0.0f);
        colorAngle = 0.261799f; // 15° rotation
    }
    else if (input.BuoyShape == BUOY_SHAPE_CAN)
    {
        atlasCoord = float2(1.0f, 0.0f);
        colorAngle = 0.261799f; // 15° rotation
    }
    else if (input.BuoyShape == BUOY_SHAPE_SPHERICAL)
    {
        atlasCoord = float2(2.0f, 0.0f);
        colorAngle = 0.261799f; // 15° rotation
    }
    else if (input.BuoyShape == BUOY_SHAPE_PILLAR)
    {
        atlasCoord = float2(3.0f, 0.0f);
        colorAngle = 0.261799f; // 15° rotation
    }
    else if (input.BuoyShape == BUOY_SHAPE_SPAR)
    {
        atlasCoord = float2(0.0f, 1.0f);
        colorAngle = 0.261799f; // 15° rotation
    }
    else if (input.BuoyShape == BUOY_SHAPE_BARREL)
    {
        atlasCoord = float2(1.0f, 1.0f);
    }
    else if (input.BuoyShape == BUOY_SHAPE_SUPER_BUOY)
    {
        atlasCoord = float2(2.0f, 1.0f);
    }
    else if (input.BuoyShape == BUOY_SHAPE_ICE_BUOY)
    {
        atlasCoord = float2(3.0f, 1.0f);
    }
	
	// only supported on shader model 4 :( maybe in the future...
	/*switch (input.BuoyShape)
	{
		case BUOY_SHAPE_CONICAL:
			atlasCoord = float2(0.0f, 0.0f);
			colorAngle = 0.261799f; // 15° rotation
			break;
		case BUOY_SHAPE_CAN:
			atlasCoord = float2(1.0f, 0.0f);
			colorAngle = 0.261799f; // 15° rotation
			break;
		case BUOY_SHAPE_SPHERICAL:
			atlasCoord = float2(2.0f, 0.0f);
			colorAngle = 0.261799f; // 15° rotation
			break;
		case BUOY_SHAPE_PILLAR:
			atlasCoord = float2(3.0f, 0.0f);
			colorAngle = 0.261799f; // 15° rotation
			break;
		case BUOY_SHAPE_SPAR:
			atlasCoord = float2(0.0f, 1.0f);
			colorAngle = 0.261799f; // 15° rotation
			break;
		case BUOY_SHAPE_BARREL:
			atlasCoord = float2(1.0f, 1.0f);
			break;
		case BUOY_SHAPE_SUPER_BUOY:
			atlasCoord = float2(2.0f, 1.0f);
			break;
		case BUOY_SHAPE_ICE_BUOY:
			atlasCoord = float2(3.0f, 1.0f);
			break;
	}*/
        atlasCoord = input.TexCoord / 4.0f + atlasCoord / 4.0f;

	float4 backColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
	if (colorCount > 0)
	{
		float2 rotatedTexCoord = float2(
			input.TexCoord.x * cos(colorAngle) - (ORIGIN_FRAC - input.TexCoord.y) * sin(colorAngle),
			(ORIGIN_FRAC - input.TexCoord.y) * cos(colorAngle) + input.TexCoord.x * sin(colorAngle));

        if (input.ColorPattern == COLOR_PATTERN_NONE)
            backColor = input.Colors[0];
        else if (input.ColorPattern == COLOR_PATTERN_HORIZONTAL)
            backColor = input.Colors[floor((1 - rotatedTexCoord.y) * colorCount)];
        else /* if (input.ColorPattern == COLOR_PATTERN_VERTICAL)*/
            backColor = input.Colors[clamp(floor(map(rotatedTexCoord.x, 0.25f, 0.75f, 0.0f, 1.0f) * colorCount), 0.0f, colorCount - 1)];
		
		// only supported on shader model 4 :( maybe in the future...
		/*switch (input.ColorPattern)
		{
			case COLOR_PATTERN_NONE:
				backColor = input.Colors[0];
				break;
			case COLOR_PATTERN_HORIZONTAL:
				backColor = input.Colors[floor((1 - rotatedTexCoord.y) * colorCount)];
				break;
			case COLOR_PATTERN_VERTICAL:
			default:
				backColor = input.Colors[clamp(floor(map(rotatedTexCoord.x, 0.25f, 0.75f, 0.0f, 1.0f) * colorCount), 0.0f, colorCount - 1)];
				break;
		}*/

	}

	float4 color = SAMPLE_TEXTURE(Texture, atlasCoord)/* * backColor */;

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