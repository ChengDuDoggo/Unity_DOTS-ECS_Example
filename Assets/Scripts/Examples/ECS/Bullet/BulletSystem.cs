using Unity.Entities;

public partial struct BulletSystem : ISystem
{
    public readonly void OnCreate(ref SystemState state)
    {
        SharedData.singtonEntity.Data = state.EntityManager.CreateEntity(typeof(BulletInfo));
    }
}