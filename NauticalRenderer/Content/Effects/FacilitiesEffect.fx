#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#include "Macros.fxh"

#define MASK_SLIPWAY              1
#define OFFSET_SLIPWAY float2(0.0f, 0.0f)

#define MASK_VISITOR_BERTH        2
#define OFFSET_VISITOR_BERTH float2(0.25f, 0.0f)

#define MASK_BOAT_HOIST           4
#define OFFSET_BOAT_HOIST float2(0.5f, 0.0f)

#define MASK_FUEL_STATION         8
#define MASK_PUMP_OUT            16
#define MASK_ELECTRICITY         32
#define MASK_BOATYARD            64
#define MASK_TOILETS            128
#define MASK_CHANDLER           256
#define MASK_SHOWERS            512
#define MASK_NAUTICAL_CLUB     1024
#define MASK_VISITORS_MOORING  2048
#define MASK_WATER_TAP         4096
#define MASK_REFUSE_BIN        8192
#define MASK_SAILMAKER        16384
#define MASK_LAUNDRETTE       32768

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
    uint Categories : TEXCOORD1;
};

uint CategoryCount(uint categories)
{
    uint v = categories;
    v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
    v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
    uint c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
    return c;
}

bool IsSet(uint value, uint bitmask)
{
    return (value & bitmask) != 0;
}

VSOutput MainVS(in VSInput input, float2 instanceTransform : POSITION1, uint categories : TEXCOORD1)
{
	VSOutput output = (VSOutput)0;
    output.Position = mul(mul(float4(instanceTransform.x, instanceTransform.y, 0, 1), WorldMatrix)
			+ float4(input.Position.x * Size * CategoryCount(categories), input.Position.y * Size, input.Position.z, 0), ViewportMatrix);
    output.TexCoord = input.TexCoord;
    output.Categories = categories;
	
	return output;
}

float4 MainPS(VSOutput input) : COLOR
{
    uint categoryCount = CategoryCount(input.Categories);
    float4 color = float4(0, 0, 0, 0); 
    float wideX = (input.TexCoord.x * categoryCount);
    float2 sizedTexCoord = float2(frac(input.TexCoord.x * categoryCount) / 4.0f,
                                  input.TexCoord.y / 4.0f);
    float2 categoryOffset = float2(1.0f / categoryCount, 0.0f);
    uint prevCount = 0;
    
    bool isSet = IsSet(input.Categories, MASK_SLIPWAY);
    color = SAMPLE_TEXTURE(Texture, sizedTexCoord + OFFSET_SLIPWAY) * isSet * ((wideX >= prevCount) && wideX < (prevCount + 1));
    prevCount += isSet;
    
    isSet = IsSet(input.Categories, MASK_VISITOR_BERTH);
    color = SAMPLE_TEXTURE(Texture, sizedTexCoord + OFFSET_VISITOR_BERTH) * isSet * ((wideX >= prevCount) && wideX < (prevCount + 1));
    prevCount += isSet;
    
    isSet = IsSet(input.Categories, MASK_VISITOR_BERTH);
    
	
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