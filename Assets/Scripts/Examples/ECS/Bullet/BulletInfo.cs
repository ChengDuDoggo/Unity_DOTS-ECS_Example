using Unity.Entities;
using Unity.Mathematics;

public struct BulletInfo : IBufferElementData
{
    public float3 position;
    public quaternion rotation;
}