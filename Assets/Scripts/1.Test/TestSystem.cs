using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

//定义一个DOTS的Aspect(封装一套数据组件,方便System直接筛选遍历增加代码可读性)
public readonly partial struct MonsterAspect : IAspect
{
    public readonly RefRW<MonsterData> monsterData;
    public readonly RefRW<LocalTransform> localTransform;
}
//定义一个DOTS的数据组件(仅仅定义存放数据)
public struct MonsterData : IComponentData
{
    public float hp;
    public float moveSpeed;
}
//定义一个DOTS的系统(实现逻辑)
public partial struct MonsterSystem : ISystem
{
    //DOTS/ECS System的三个生命周期函数
    public void OnCreate(ref SystemState state)
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
        //筛选遍历
        float3 dir = new(0, 0, 1);//方向
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
            monster.localTransform.ValueRW.Position += monster.monsterData.ValueRW.moveSpeed * SystemAPI.Time.DeltaTime * dir;//移动
            monster.monsterData.ValueRW.hp -= SystemAPI.Time.DeltaTime;//掉血
        }
    }
    public readonly void OnDestroy()
    {

    }
}