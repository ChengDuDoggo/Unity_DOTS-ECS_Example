using Unity.Entities;

public struct BulletSharedInfo : ISharedComponentData
{
    public float moveSpeed;
    public float destroyTime;
}