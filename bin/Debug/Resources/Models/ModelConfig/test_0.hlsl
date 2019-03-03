matrix GlobalWVP;

struct VtxInput
{
    float4 VtxPosition : POSITION;
    float4 VtxNormal : NORMAL;
    float4 VtxColor : COLOR;
};

struct VS2PS
{
    float4 VtxPosition : SV_POSITION;
    float4 VtxNormal : NORMAL;
    float4 VtxColor : COLOR;
};

VS2PS VShader(VtxInput dataIn)
{
    dataIn.VtxPosition.w = 1.0f;
    VS2PS result;
    result.VtxPosition = mul(GlobalWVP,dataIn.VtxPosition);
    result.VtxPosition.z /= result.VtxPosition.w;
    result.VtxPosition.z = result.VtxPosition.z - 1.0f;
    result.VtxNormal = dataIn.VtxNormal;
    float ht = 0.15 + 0.1 * dataIn.VtxPosition.z / 20.0f;
    result.VtxColor = float4(ht,ht,ht,1.0f);
	return result;
}

float4 PShader(VS2PS dataIn) : SV_Target
{
    float tmpV = 0.5 - dataIn.VtxNormal.x * 0.1;
    if (dataIn.VtxNormal.z == 1.0f)
    {
        tmpV += dataIn.VtxColor.x;
    }
    return float4(tmpV, tmpV, tmpV, 1.0f);
}

technique10 myTechnique
{
	pass myPass
	{
		SetVertexShader(CompileShader(vs_5_0, VShader()));
		SetPixelShader(CompileShader(ps_5_0, PShader()));
	}
}