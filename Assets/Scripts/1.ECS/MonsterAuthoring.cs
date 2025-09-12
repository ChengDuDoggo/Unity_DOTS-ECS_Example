using Unity.Entities;
using UnityEngine;

public class MonsterAuthoring : MonoBehaviour
{
    public float hp = 100;
    public float moveSpeed = 1;
    public float createBulletInterval = 1;//创建子弹的间隔时间
    public GameObject bulletPrefab;
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
                bulletPrototype = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),//获取Mono子弹预制体对应的ECS实体
                createBulletInterval = authoring.createBulletInterval,
            });
            AddComponent<MoveData>(monsterEntity, new MoveData()
            {
                moveSpeed = authoring.moveSpeed
            });
        }
    }
}