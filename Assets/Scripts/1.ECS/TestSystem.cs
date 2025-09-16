using System.Threading;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//动态缓冲区数据组件(可以理解为Mono中的List)
//要作为数据组件单独创建
public struct Skill : IBufferElementData
{
    public int Id;
}
//定义一个DOTS的Aspect(封装一套数据组件,方便System直接筛选遍历增加代码可读性)
public readonly partial struct MonsterAspect : IAspect
{
    public readonly RefRW<MonsterData> monsterData;
    public readonly RefRW<LocalTransform> localTransform;
    //因为共享数据组件是必然只读的。因此不需要RefRW或RefRO读写包装
    public readonly MonsterSharedConfigData monsterSharedConfigData;
    public readonly DynamicBuffer<Skill> skillBuffer;//动态缓冲区数据组件
}
public readonly partial struct MoveAspect : IAspect
{
    public readonly RefRW<MoveData> moveData;
    public readonly RefRW<LocalTransform> localTransform;
}
//定义一个DOTS的数据组件(仅仅定义存放数据)
//IEnableableComponent:可以让该数据组件支持启用和禁用功能
public struct MonsterData : IComponentData, IEnableableComponent
{
    public float hp;
    public float createBulletTimer;//创建子弹的计时器(默认为0不需要从Mono去Baker)
    //public float createBulletInterval;//创建子弹的间隔时间
}
//定义一个DOTS的共享数据组件(对于一个挂载在多个实体对象上的数据组件,如果其中有字段的数据或者值在所有实体上都是固定的不变的,那么我们可以将这些值单独分割出来放入共享数据组件中(可以理解为Mono中的常量配置Config))
//踩坑(预制体实体字段不能存放在共享数据组件中,否则序列化会报错)
//注意!共享组件并不共享数据,只是相同数据的实体会被放在一起,提高缓存命中率,提高性能
public struct MonsterSharedConfigData : ISharedComponentData
{
    public float createBulletInterval;//创建子弹的间隔时间
}
public struct MoveData : IComponentData
{
    public float moveSpeed;
}
public struct GameManagerData : IComponentData
{
    public Entity bulletPrototype;//子弹原型(就是Mono中的预制体)
}
//定义一个DOTS的系统(实现逻辑)
public partial struct MonsterSystem : ISystem
{
    //DOTS/ECS System的三个生命周期函数
    public readonly void OnCreate(ref SystemState state)
    {
        //正常来说不会在这里直接创建实体,而是通过在Unity中创建MonoBehaviour，Authoring(Authoring是MonoBehaviour的类)和Baker(烘焙器)来创建实体
        //创建一个Monster Entity(实体)并且在创建时添加MonsterData和LocalTransform(Unity自带)数据组件
        //Entity monster = state.EntityManager.CreateEntity(typeof(MonsterData), typeof(LocalTransform));
        //设置实体的MonsterData数据组件的hp值为100
        //state.EntityManager.SetComponentData(monster, new MonsterData { hp = 100 });
        //Baker方式烘培了一个Mono,直接将Mono转换为一个实体,System直接可以筛选遍历，不需要再创建实体操作
    }
    public readonly void OnUpdate(ref SystemState state)
    {
        //SystemAPI:只能在System中使用,否则会报错
        //获取单例数据组件
        //假如某一个数据组件全局只有一个,可以直接通过GetSingleton获取该数据组件,不再进行筛选遍历
        SystemAPI.TryGetSingleton<GameManagerData>(out GameManagerData gameManagerData);//获取单例数据组件
        //筛选遍历拥有MonsterData和LocalTransform数据组件的实体
        //foreach ((RefRW<MonsterData> monsterData, RefRW<LocalTransform> localTransfrom) in SystemAPI.Query<RefRW<MonsterData>, RefRW<LocalTransform>>())
        //{
        //    //读写其中的数据组件以更改数据实现逻辑
        //    localTransfrom.ValueRW.Position += dir * SystemAPI.Time.DeltaTime;//移动
        //    monsterData.ValueRW.hp -= SystemAPI.Time.DeltaTime;//掉血
        //}
        //使用封装数据组件的方式筛选遍历
        //foreach (MonsterAspect monster in SystemAPI.Query<MonsterAspect>())
        //{
        //monster.monsterData.ValueRW.hp -= SystemAPI.Time.DeltaTime;//掉血
        //monster.monsterData.ValueRW.createBulletTimer -= SystemAPI.Time.DeltaTime;//计时
        //if (monster.monsterData.ValueRO.createBulletTimer <= 0)//可以创建子弹
        //{
        //    monster.monsterData.ValueRW.createBulletTimer = monster.monsterSharedConfigData.createBulletInterval;//重置计时器
        //    Entity bullet = state.EntityManager.Instantiate(gameManagerData.bulletPrototype);//创建子弹实体
        //    state.EntityManager.SetComponentData(bullet, new LocalTransform()//设置子弹实体的LocalTransform数据组件的数据
        //    {
        //        Position = monster.localTransform.ValueRO.Position,//设置子弹位置为怪物位置
        //        Scale = 0.5f//设置子弹大小缩放
        //    });
        //}
        //}

        //命令缓存区(ECB)
        //因为在IJobEntity中不能直接操作EntityManager,所以在IJobEntity中我们不能直接执行创建实体、销毁实体、添加数据组件、移除数据组件等操作
        //此时我们就需要使用命令缓存区(EntityCommandBuffer)来缓存这些操作命令,等Job执行完毕后再统一执行这些操作

        //方式一:传统方式创建ECB(自己New)
        //EntityCommandBuffer ecb = new(Allocator.TempJob);

        //方式二:通过系统单例获得ECB(推荐)
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        //使用多线程Job方式,处理拥有MonsterAspect数据组件的实体
        //IJobEntity会根据Execute函数的参数自动筛选拥有这些数据组件的实体
        new MonsterJob() { ecb = ecb, gameManagerData = gameManagerData, deltaTime = SystemAPI.Time.DeltaTime }.Schedule();
        //直接单例获得系统的ECB无需任何操作
        //传统ECB方式需要执行以下操作
        //state.Dependency.Complete();//确保Job执行完毕
        //ecb.Playback(state.EntityManager);//执行命令缓存区中的命令
        //ecb.Dispose();//释放命令缓存区
    }
    public partial struct MonsterJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public GameManagerData gameManagerData;
        public float deltaTime;
        //Execute函数的参数会自动筛选拥有这些数据组件的实体(MonsterAspect和Entity)
        public readonly void Execute(MonsterAspect monsterAspect)
        {
            monsterAspect.monsterData.ValueRW.hp -= deltaTime;//掉血
            monsterAspect.monsterData.ValueRW.createBulletTimer -= deltaTime;//计时
            if (monsterAspect.monsterData.ValueRO.createBulletTimer <= 0)//可以创建子弹
            {
                monsterAspect.monsterData.ValueRW.createBulletTimer = monsterAspect.monsterSharedConfigData.createBulletInterval;//重置计时器
                Entity bullet = ecb.Instantiate(gameManagerData.bulletPrototype);//创建子弹实体
                ecb.SetComponent(bullet, new LocalTransform()//设置子弹实体的LocalTransform数据组件的数据
                {
                    Position = monsterAspect.localTransform.ValueRO.Position,//设置子弹位置为怪物位置
                    Scale = 0.5f//设置子弹大小缩放
                });
            }
        }
    }
    public readonly void OnDestroy()
    {

    }
}
public partial struct MoveSystem : ISystem
{
    public readonly void OnUpdate(ref SystemState state)
    {
        //常规SystemAPI.Query方式,循环遍历寻找拥有MoveAspect数据组件的实体
        float3 dir = new(0, 0, 1);
        //foreach (MoveAspect monster in SystemAPI.Query<MoveAspect>())
        //{
        //    monster.localTransform.ValueRW.Position += monster.moveData.ValueRW.moveSpeed * SystemAPI.Time.DeltaTime * dir;//移动
        //}
        //使用多线程Job方式,并行处理拥有MoveAspect数据组件的实体
        //IJobEntity会根据Execute函数的参数自动筛选拥有这些数据组件的实体
        new MoveJob() { deltaTime = SystemAPI.Time.DeltaTime, dir = dir }.ScheduleParallel();
    }
    public partial struct MoveJob : IJobEntity
    {
        public float deltaTime;
        public float3 dir;
        //Execute函数的参数会自动筛选拥有这些数据组件的实体(MoveAspect和Entity)
        public readonly void Execute(MoveAspect moveAspect, Entity entity)
        {
            moveAspect.localTransform.ValueRW.Position += moveAspect.moveData.ValueRW.moveSpeed * deltaTime * dir;//移动
            Debug.Log("MoveJob线程:" + Thread.CurrentThread.ManagedThreadId + "||Entity:" + entity);
        }
    }
}