using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public partial struct BulletSystem : ISystem
{
    //需要创建子弹数量
    public readonly static SharedStatic<int> createBulletCount = SharedStatic<int>.GetOrCreate<BulletSystem>();
    public readonly void OnCreate(ref SystemState state)
    {
        createBulletCount.Data = 0;
        SharedData.singtonEntity.Data = state.EntityManager.CreateEntity(typeof(BulletCreateInfo));
    }
    public readonly void OnUpdate(ref SystemState state)
    {
        //获取系统ECB
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        DynamicBuffer<BulletCreateInfo> bulletCreateInfoBuffer = SystemAPI.GetSingletonBuffer<BulletCreateInfo>();
        createBulletCount.Data = bulletCreateInfoBuffer.Length;
        new BulletJob()
        {
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
            bulletCreateInfoBuffer = bulletCreateInfoBuffer
        }.ScheduleParallel();
        //确保Job执行完毕再执行之下逻辑
        state.CompleteDependency();
        //补充不足的部分
        if (createBulletCount.Data > 0)
        {
            NativeArray<Entity> newBullets = new(createBulletCount.Data, Allocator.Temp);
            ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().bulletPortotype, newBullets);
            for (int i = 0; i < newBullets.Length; i++)
            {
                BulletCreateInfo info = bulletCreateInfoBuffer[i];
                ecb.SetComponent<LocalTransform>(newBullets[i].Index, newBullets[i], new LocalTransform()
                {
                    Position = info.position,
                    Rotation = info.rotation,
                    Scale = 1.0f
                });
            }
            newBullets.Dispose();
        }
        //子弹生成后清空缓冲区，反复循环
        bulletCreateInfoBuffer.Clear();
    }

    //设定忽略组件Enable状态，这样即使有的数据组件是禁用状态，Job依然可以遍历查找到它
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    [BurstCompile]
    public partial struct BulletJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        [ReadOnly]
        public DynamicBuffer<BulletCreateInfo> bulletCreateInfoBuffer;
        public float deltaTime;
        public readonly void Execute(EnabledRefRW<BulletData>/*控制数据组件激活与关闭的参数*/ bulletEnableState, EnabledRefRW<RendererSortTag> sortEnableState, ref BulletData bulletData, ref LocalTransform localTransform, in BulletSharedInfo bulletSharedInfo, in Entity entity)
        {
            //当前子弹是非激活状态，同时需要创建子弹
            if (bulletEnableState.ValueRO == false)
            {
                if (BulletSystem.createBulletCount.Data > 0)
                {
                    int index = createBulletCount.Data -= 1;
                    bulletEnableState.ValueRW = true;
                    sortEnableState.ValueRW = true;
                    localTransform.Position = bulletCreateInfoBuffer[index].position;
                    localTransform.Rotation = bulletCreateInfoBuffer[index].rotation;
                    localTransform.Scale = 1.0f;
                    bulletData.destroyTime = bulletSharedInfo.destroyTime;
                }
                return;
            }
            //位置移动
            localTransform.Position += bulletSharedInfo.moveSpeed * deltaTime * localTransform.Up();
            //时间销毁
            bulletData.destroyTime -= deltaTime;
            if (bulletData.destroyTime <= 0)
            {
                bulletEnableState.ValueRW = false;
                sortEnableState.ValueRW = false;
                localTransform.Scale = 0;
                return;
            }
        }
    }
}