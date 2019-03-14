matrix GlobalWVP;
float4 CursorInput;
int SelectedObjectId;
float ScreenHHeight;
float ScreenHWidth;
float Token1;

struct VtxInput
{
    float4 VtxPosition : POSITION;
    float4 VtxNormal : NORMAL;
    float4 VtxColor : COLOR;
    uint Tag : Output;
};

struct VS2PS
{
    float4 VtxPosition : SV_POSITION;
    float4 VtxColor : COLOR;
    uint Tag : Output;
};

VS2PS VShader(VtxInput dataIn)
{   
    dataIn.VtxPosition.w = 1.0f;
    VS2PS result;
    result.VtxPosition = mul(GlobalWVP,dataIn.VtxPosition);
    result.VtxPosition.z /= result.VtxPosition.w;

    result.VtxPosition.x /= result.VtxPosition.w;
    result.VtxPosition.y /= result.VtxPosition.w;
    result.VtxPosition.w = 1.0f;
    result.Tag = dataIn.Tag;
    float ht = 0.35 - dataIn.VtxNormal.x * 0.1;
    if (dataIn.VtxNormal.z == 1)
    {
        ht += 0.05 * dataIn.VtxPosition.z / 20.0f;
    }
    ht *= 0.5;
    //if (dataIn.Tag == SelectedObjectId)
    //{
    //    ht += 0.2;
    //}
    result.VtxColor = float4(ht, ht, ht, 1.0f);
	return result;
}

[maxvertexcount(6)]
void GShader(triangle VS2PS input[3], inout TriangleStream<VS2PS> output)
{
    // calculate if the cursor is in the triangle
    float highLight = 0.0f;
    float tmpZ = 1.0f;
    if (true)  //CursorInput.z
    {
        
        float s1 = abs((input[0].VtxPosition.x  - CursorInput.x) * (input[2].VtxPosition.y  - CursorInput.y) - (input[0].VtxPosition.y  - CursorInput.y) * (input[2].VtxPosition.x  - CursorInput.x));
        float s2 = abs((input[0].VtxPosition.x  - CursorInput.x) * (input[1].VtxPosition.y  - CursorInput.y) - (input[0].VtxPosition.y  - CursorInput.y) * (input[1].VtxPosition.x  - CursorInput.x));
        float s3 = abs((input[1].VtxPosition.x  - CursorInput.x) * (input[2].VtxPosition.y  - CursorInput.y) - (input[1].VtxPosition.y  - CursorInput.y) * (input[2].VtxPosition.x  - CursorInput.x));
        float s0 = abs((input[1].VtxPosition.x  - input[0].VtxPosition.x ) * (input[2].VtxPosition.y  - input[0].VtxPosition.y ) - (input[1].VtxPosition.y  - input[0].VtxPosition.y ) * (input[2].VtxPosition.x  - input[0].VtxPosition.x ));

        if (s1 + s2 + s3 <= s0 + 0.0001)
        {
            highLight = 0.5f;
            float tmpNormalX = (input[1].VtxPosition.y - input[0].VtxPosition.y) * (input[2].VtxPosition.z - input[0].VtxPosition.z) - (input[1].VtxPosition.z - input[0].VtxPosition.z) * (input[2].VtxPosition.y - input[0].VtxPosition.y);
            float tmpNormalY = -(input[1].VtxPosition.x - input[0].VtxPosition.x) * (input[2].VtxPosition.z - input[0].VtxPosition.z) + (input[1].VtxPosition.z - input[0].VtxPosition.z) * (input[2].VtxPosition.x - input[0].VtxPosition.x);
            float tmpNormalZ = (input[1].VtxPosition.x - input[0].VtxPosition.x) * (input[2].VtxPosition.y - input[0].VtxPosition.y) - (input[1].VtxPosition.y - input[0].VtxPosition.y) * (input[2].VtxPosition.x - input[0].VtxPosition.x);

            float tmpConst = tmpNormalX * input[0].VtxPosition.x + tmpNormalY * input[0].VtxPosition.y + tmpNormalZ * input[0].VtxPosition.z;
            tmpZ = (tmpConst - tmpNormalX * CursorInput.x - tmpNormalY * CursorInput.y) / tmpNormalZ;
            if (tmpZ >= 0.0f && tmpZ <= 1.0f)
            {
                tmpZ = 0.5 + 0.5 * tmpZ;
            }

        }
    }

    for (uint i = 0; i < 3; i++)
    {
        VS2PS element;
        element.VtxPosition = input[i].VtxPosition;
        //float tmpColor = input[i].VtxColor.r + highLight;
        //element.VtxColor = float4(tmpColor, tmpColor, tmpColor, 1);
        element.VtxColor = input[i].VtxColor;
        if (input[i].Tag == SelectedObjectId)
        {
            float4 yellow = float4(0.93f, 0.91f, 0.67f, 1.0f);
            element.VtxColor = float4(input[i].VtxColor.r + 0.5 * yellow.r, input[i].VtxColor.g + 0.5 * yellow.g, input[i].VtxColor.b + 0.5 * yellow.b, 1.0f);
        }
        element.Tag = input[i].Tag;
        output.Append(element);
    }

    if (highLight > 0)
    {
        output.RestartStrip();
        int id = input[0].Tag;
        float pixel_h = 2.0f / ScreenHHeight;
        float pixel_w = 2.0f / ScreenHWidth;
        VS2PS ele[3];
        for (uint j = 0; j < 3; j++)
        {
            VS2PS element;
            element.Tag = id;
            element.VtxColor = float4(0.0f, ((id / 256) % 255) / 255.0f, (id % 256) / 255.0f, 1.0f);
            ele[j] = element;
        }
        ele[0].VtxPosition = float4(-1.0f, 1.0f - pixel_h, tmpZ, 1.0f);
        ele[1].VtxPosition = float4(-1.0f + pixel_w, 1.0f, tmpZ, 1.0f);
        ele[2].VtxPosition = float4(-1.0f, 1.0f, tmpZ, 1.0f);

        output.Append(ele[2]);
        output.Append(ele[0]);
        output.Append(ele[1]);

    }
    //output.RestartStrip();
    
    //VS2PS ele[3];

    //for (uint j = 0; j < 3; j++)
    //{
    //    VS2PS element;
    //    if (j == 0)
    //    {
    //        element.VtxPosition = float4(-0.9f, -0.9f, 0.5f, 1.0f);
    //    }
    //    else if (j == 1)
    //    {
    //        element.VtxPosition = float4(-0.95f, -0.9f, 0.5f, 1.0f);
    //    }
    //    else
    //    {
            
    //        element.VtxPosition = float4(-0.9f, -0.95f, 0.5f, 1.0f);
    //    }

    //    element.VtxNormal = input[j].VtxNormal;
    //    element.VtxColor = float4(0.0f,0.0f,1.0f,1.0f);
    //    ele[j] = element;
    //}
    
    //output.Append(ele[1]);
    //output.Append(ele[2]);
    //output.Append(ele[0]);

}

float4 PShader(VS2PS dataIn) : SV_Target
{
    //float tmpV = 0.5 - dataIn.VtxNormal.x * 0.1;
    //if (dataIn.VtxNormal.z == 1.0f)
    //{
    //    tmpV += dataIn.VtxColor.x;
    //}
    //return float4(tmpV, tmpV, tmpV, 1.0f);

    return dataIn.VtxColor;
}

technique10 myTechnique
{
	pass myPass
	{
		SetVertexShader(CompileShader(vs_5_0, VShader()));
        SetGeometryShader(CompileShader(gs_5_0, GShader()));
		SetPixelShader(CompileShader(ps_5_0, PShader()));
	}
}