using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

public partial struct RendererSortSystem : ISystem
{
    public readonly void OnUpdate(ref SystemState state)
    {
        new RendererSortJob().ScheduleParallel();
    }
    [BurstCompile]
    public partial struct RendererSortJob : IJobEntity
    {
        public readonly void Execute(in RendererSortTag sortTag, ref LocalTransform localTransform)
        {
            localTransform.Position.z = localTransform.Position.y;
        }
    }
}