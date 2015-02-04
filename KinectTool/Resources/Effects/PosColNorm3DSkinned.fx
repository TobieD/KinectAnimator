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

PS_INPUT VS(VS_INPUT i){

	PS_INPUT o;
	
	float4 origPos = float4(i.pos,1.0f);
	float4 transformedPos = origPos;
	float3 transformedNormal = i.normal;
	float4 bi = i.blendIndices;
	float4 bw = i.blendWeights;	
	
	//4 vertices connect
	for(int	x = 0; x < 3; ++x)
	{
		float blendI = bi[x];
		if(blendI > -1)
		{
			float4x4 boneTransform = _Bones[blendI];
			float weight = bw[x];
			
			//update pos
			transformedPos += mul(origPos,boneTransform) * weight;
			
			//update normal rotation
			transformedNormal += mul(i.normal,(float3x3)boneTransform) * weight;	
		
		}
	}
	transformedPos.w = 1;
	
	
	o.pos = mul ( transformedPos, _WorldViewProj );
	o.normal = mul(normalize(transformedNormal).xyz, (float3x3)_World);
	o.color = i.color;
	
	return o;
}

float4 PS(PS_INPUT i) : SV_TARGET
{	
	i.normal = normalize(i.normal);
	float3 color= i.color.rgb;
	float a = i.color.a;
	
	//HalfLambert Diffuse :)
	float diffuseStrength = dot(i.normal, -_Light);
	diffuseStrength = diffuseStrength * 0.5 + 0.5;
	diffuseStrength = saturate(diffuseStrength);

	color = color * diffuseStrength;
	
	return float4( color , a );
}

technique10 Default
{
    pass P0
    {
		SetRasterizerState(Solid);
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}

