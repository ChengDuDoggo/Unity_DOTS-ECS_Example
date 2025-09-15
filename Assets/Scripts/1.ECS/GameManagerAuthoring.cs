using Unity.Entities;
using UnityEngine;

public class GameManagerAuthoring : MonoBehaviour
{
    public GameObject bulletPrefab;
    public class GameManagerBaker : Baker<GameManagerAuthoring>
    {
        public override void Bake(GameManagerAuthoring authoring)
        {
            //创建一个实体(空实体)
            Entity gameManagerEntity = GetEntity(TransformUsageFlags.None);
            //给实体添加数据组件
            AddComponent(gameManagerEntity, new GameManagerData()
            {
                bulletPrototype = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic)//将预制体转换为实体(获取Mono子弹预制体对应的ECS实体)
            });
        }
    }
}