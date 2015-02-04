///-------------------------------
/// Skinned Mesh Shader with basic colors
///
///-------------------------------

float4x4 _World : WORLD;
float4x4 _WorldViewProj : WORLDVIEWPROJECTION; 
float3 _Light = float3(-0.577f, -0.577f, 0.577f);
float4x4 _Bones[50]; //max 50 bones


RasterizerState Solid
{
	FillMode = SOLID;
	CullMode = FRONT;
};

struct VS_INPUT{
	float3 pos : POSITION;
	float3 normal : NORMAL;
	float4 color : COLOR;
	float4 blendIndices: BLENDINDICES;
	float4 blendWeights: BLENDWEIGHTS;
};

struct PS_INPUT{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL;
	float4 color : COLOR;
};

PS_INPUT VS(VS_INPUT input){

	PS_INPUT output;

	
	return output;
}

float4 PS(PS_INPUT input) : SV_TARGET{

	
	return float4( color_rgb , color_a );
}

technique11 Default
{
    pass P0
    {
		SetRasterizerState(Solid);
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}

