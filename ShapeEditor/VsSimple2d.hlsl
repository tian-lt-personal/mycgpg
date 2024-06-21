#include "Types.hlsli"

VsSimpleResult main(VertexSimple2d v)
{
    VsSimpleResult result;
    result.pos = float4(v.pos, 0.0f, 0.0f);
    result.color = v.color;
    return result;
}

