using Unity.Entities;
using UnityEngine;

public class MonsterAuthoring : MonoBehaviour
{
    public float hp = 100;
    public float moveSpeed = 1;
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
                moveSpeed = authoring.moveSpeed
            });
        }
    }
}