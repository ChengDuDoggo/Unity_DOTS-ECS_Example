using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct EnemySystem : ISystem
{
    public struct Key1 { }
    public struct Key2 { }
    public struct Key3 { }
    public readonly static SharedStatic<int> createdCount = SharedStatic<int>.GetOrCreate<Key1>();
    public readonly static SharedStatic<int> createCount = SharedStatic<int>.GetOrCreate<Key2>();
    public readonly static SharedStatic<Random> random = SharedStatic<Random>.GetOrCreate<Key3>();
    public float spawnEnemyTimer;
    public const int maxSpawnEnemyCount = 10000;
    public readonly void OnCreate(ref SystemState state)
    {
        createdCount.Data = 0;
        createCount.Data = 0;
        random.Data = new Random((uint)System.DateTime.Now.GetHashCode());
    }
    public void OnUpdate(ref SystemState state)
    {
        spawnEnemyTimer -= SystemAPI.Time.DeltaTime;
        if (spawnEnemyTimer <= 0)
        {
            spawnEnemyTimer = SharedData.spawnEnemySharedData.Data.spawnInterval;
            createCount.Data = SharedData.spawnEnemySharedData.Data.spawnCount;
        }
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        float2 playerPos = SharedData.playerPos.Data;
        new EnemyJob()
        {
            playerPos = playerPos,
            ecb = ecb
        }.ScheduleParallel();
        state.CompleteDependency();
        if (createCount.Data > 0 && createdCount.Data < maxSpawnEnemyCount)
        {
            NativeArray<Entity> newEnemy = new(createCount.Data, Allocator.Temp);
            ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().enemyPortotype, newEnemy);
            for (int i = 0; i < newEnemy.Length && createdCount.Data < maxSpawnEnemyCount; i++)
            {
                createdCount.Data++;
                float2 offset = random.Data.NextFloat2Direction() * random.Data.NextFloat(5.0f, 10.0f);
                ecb.SetComponent<LocalTransform>(newEnemy[i].Index, newEnemy[i], new LocalTransform()
                {
                    Position = new float3(playerPos + offset, 0),
                    Rotation = quaternion.identity,
                    Scale = 1.0f
                });
            }
            createCount.Data = 0;
            newEnemy.Dispose();
        }
    }
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    [BurstCompile]
    public partial struct EnemyJob : IJobEntity
    {
        public float2 playerPos;
        public EntityCommandBuffer.ParallelWriter ecb;
        public readonly void Execute(EnabledRefRW<EnemyData> enableState, EnabledRefRW<RendererSortTag> rendererSortEnableState, EnabledRefRW<AnimationFrameIndex> animationFrameIndex, ref EnemyData enemyData, EnemySharedData enemySharedData, ref LocalTransform localTransform)
        {
            if (enableState.ValueRO == false)
            {
                if (createCount.Data > 0)
                {
                    createCount.Data--;
                    float2 offset = random.Data.NextFloat2Direction() * random.Data.NextFloat(5.0f, 10.0f);
                    localTransform.Position = new float3(playerPos + offset, 0);
                    enableState.ValueRW = true;
                    rendererSortEnableState.ValueRW = true;
                    animationFrameIndex.ValueRW = true;
                }
                return;
            }
        }
    }
}