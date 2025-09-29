using Unity.Burst;
using Unity.Entities;

public partial struct AnimationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        new AnimationJob
        {
            deltaTime = state.WorldUnmanaged.Time.DeltaTime
        }.ScheduleParallel();
    }
    [BurstCompile]
    public partial struct AnimationJob : IJobEntity
    {
        public float deltaTime;
        public readonly void Execute(ref AnimationFrameIndex frameIndex, in AnimationSharedData animationData)
        {
            float newIndex = frameIndex.value + animationData.frameRate * deltaTime;
            while (newIndex > animationData.frameMaxIndex)
            {
                newIndex -= animationData.frameMaxIndex;
            }
            frameIndex.value = newIndex;
        }
    }
}