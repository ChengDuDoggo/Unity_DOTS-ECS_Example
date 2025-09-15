using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MonsterAuthoring : MonoBehaviour
{
    public float hp = 100;
    public float moveSpeed = 1;
    public float createBulletInterval = 1;//创建子弹的间隔时间
    public List<int> monsterSkillIds = new();
    //烘培器(将MonoMonoBehaviour对象烘培到ECS中)
    public class MonsterBaker : Baker<MonsterAuthoring>
    {
        public override void Bake(MonsterAuthoring authoring)
        {
            //获取实体(该实体是动态的,因此TransformUsageFlags.Dynamic,Dynamic会自动帮助实体添加LocalTransform数据组件)
            Entity monsterEntity = GetEntity(TransformUsageFlags.Dynamic);
            //增加数据组件并赋值
            AddComponent<MonsterData>(monsterEntity, new MonsterData()
            {
                hp = authoring.hp,
                //createBulletInterval = authoring.createBulletInterval,
            });
            //因为MonsterData实现了IEnableableComponent接口,因此可以对该数据组件进行启用和禁用操作
            SetComponentEnabled<MonsterData>(monsterEntity, true);
            //增加共享数据组件并赋值
            AddSharedComponent(monsterEntity, new MonsterSharedConfigData()
            {
                createBulletInterval = authoring.createBulletInterval,
            });
            AddComponent<MoveData>(monsterEntity, new MoveData()
            {
                moveSpeed = authoring.moveSpeed
            });
            //为该实体增加动态缓冲区数据组件(List)
            AddBuffer<Skill>(monsterEntity);
            //为Monster实体的动态缓冲区数据组件添加数据(赋值)
            for(int i = 0; i < authoring.monsterSkillIds.Count; i++)
            {
                AppendToBuffer(monsterEntity, new Skill { Id = authoring.monsterSkillIds[i] });
            }
        }
    }
}