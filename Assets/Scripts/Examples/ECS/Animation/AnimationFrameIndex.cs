using Unity.Entities;
using Unity.Rendering;

[MaterialProperty("_Index")]
public struct AnimationFrameIndex : IComponentData
{
    public float value;
}