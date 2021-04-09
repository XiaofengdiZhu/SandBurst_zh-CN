float scale;
float a;
float params[5];
float2 texSize;

struct VS_INPUT
{
	float4 vPosition		: POSITION;			//頂点座標
	float2 vTexCoords		: TEXCOORD0;		//テクスチャUV
};

struct VS_OUTPUT
{
	float4 vPosition		: POSITION;			//頂点座標
	float2 vTexCoords		: TEXCOORD0;		//テクスチャUV
};

struct PS_INPUT
{
	float2 vTexCoords		: TEXCOORD0;		//テクスチャUV
};

struct PS_OUTPUT
{
	float4 vColor			: COLOR0;			//最終的な出力色
};

texture gTexture0;

// 頂点シェーダ
VS_OUTPUT BasicTransform( VS_INPUT v )
{
   return v;
}

sampler bicubicSampler = sampler_state
{
    Texture   = <gTexture0>;
    MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};

sampler linearSampler = sampler_state
{
    Texture   = <gTexture0>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

// ピクセルシェーダ
PS_OUTPUT BicubicPixelShader( PS_INPUT p )
{
    PS_OUTPUT o = (PS_OUTPUT)0;

	float4 color = float4(0, 0 ,0, 0);
	float x = p.vTexCoords.x * texSize.x;
	float y = p.vTexCoords.y * texSize.y;
	float invWidth = 1.0 / texSize.x;
	float invHeight = 1.0 / texSize.y;
	float x0 = x;
	float y0 = y;
	int xBase = (int)x0;
	int yBase = (int)y0;

	for (int i = -1; i <= 2; i++)
	{
		for (int j = -1; j <= 2; j++)
		{
			float xCurrent = (float)(xBase + i);
			float yCurrent = (float)(yBase + j);
			
			float distX = abs(xCurrent - x0);
			float distY = abs(yCurrent - y0);
			
			float weight = 0.0;
			
			if (distX <= 1.0)
			{
				weight = 1.0 - params[0] * distX*distX + params[1] * distX*distX*distX;
			}
			else if (distX <= 2.0) 
			{
				weight = params[2] + params[3] * distX - params[4] * distX*distX + a * distX*distX*distX;
			}
						
			if (distY <= 1.0) 
			{
				weight *= 1.0 - params[0] * distY*distY + params[1] * distY*distY*distY;
			}
			else if (distY <= 2.0) 
			{
				weight *= params[2] + params[3] * distY - params[4] * distY*distY + a * distY*distY*distY;
			}
			
			float4 colorProcess = tex2D(bicubicSampler, float2(xCurrent * invWidth, yCurrent * invHeight));
			color += colorProcess * weight;
		}
	}
	
	o.vColor = clamp(color, 0, 1.0);

    return o;
}

// ピクセルシェーダ
PS_OUTPUT BasicPixelShader( PS_INPUT p )
{
    PS_OUTPUT o = (PS_OUTPUT)0;
	o.vColor = tex2D(linearSampler, p.vTexCoords);
	return o;
}

technique BasicTec
{
   pass P0
   {
      VertexShader = compile vs_3_0 BasicTransform();
      PixelShader = compile ps_3_0 BasicPixelShader();
   }
}

technique BicubicTec
{
   pass P0
   {
      VertexShader = compile vs_3_0 BasicTransform();
      PixelShader = compile ps_3_0 BicubicPixelShader();
   }
}