using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
        foreach (MonsterAspect monster in SystemAPI.Query<MonsterAspect>())
        {
            monster.monsterData.ValueRW.hp -= SystemAPI.Time.DeltaTime;//掉血
            monster.monsterData.ValueRW.createBulletTimer -= SystemAPI.Time.DeltaTime;//计时
            if (monster.monsterData.ValueRO.createBulletTimer <= 0)//可以创建子弹
            {
                monster.monsterData.ValueRW.createBulletTimer = monster.monsterSharedConfigData.createBulletInterval;//重置计时器
                Entity bullet = state.EntityManager.Instantiate(gameManagerData.bulletPrototype);//创建子弹实体
                state.EntityManager.SetComponentData(bullet, new LocalTransform()//设置子弹实体的LocalTransform数据组件的数据
                {
                    Position = monster.localTransform.ValueRO.Position,//设置子弹位置为怪物位置
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
        float3 dir = new(0, 0, 1);
        foreach (MoveAspect monster in SystemAPI.Query<MoveAspect>())
        {
            monster.localTransform.ValueRW.Position += monster.moveData.ValueRW.moveSpeed * SystemAPI.Time.DeltaTime * dir;//移动
        }
    }
}