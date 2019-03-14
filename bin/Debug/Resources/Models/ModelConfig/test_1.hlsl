matrix GlobalWVP;

struct VtxInput2
{
    float4 VtxPosition : POSITION;
    float4 VtxNormal : NORMAL;
    float4 VtxColor : COLOR;
};

struct VS2PS2
{
    float4 VtxPosition : SV_POSITION;
    float4 VtxColor : COLOR;
};

VS2PS2 VShader(VtxInput2 dataIn)
{   
    dataIn.VtxPosition.w = 1.0f;
    VS2PS2 result;
    result.VtxPosition = mul(GlobalWVP,dataIn.VtxPosition);

    float lightArg = 0.0f;
    if (dataIn.VtxNormal.x > 0)
    {
        lightArg += 0.577 * dataIn.VtxNormal.x;
    }
    if (dataIn.VtxNormal.y < 0)
    {
        lightArg += -0.577 * dataIn.VtxNormal.y;
    }
    if (dataIn.VtxNormal.z > 0)
    {
        lightArg += 0.577 * dataIn.VtxNormal.z;
    }

    float4 light = float4(lightArg, lightArg, lightArg, 1.0f);
    result.VtxColor = dataIn.VtxColor*light;
	return result;
}

float4 PShader(VS2PS2 dataIn) : SV_Target
{
    return dataIn.VtxColor;
}

technique10 myTechnique1
{
	pass myPass1
	{
        SetVertexShader(CompileShader(vs_5_0, VShader()));
        SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PShader()));
	}
}