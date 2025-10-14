using Unity.Entities;
using UnityEngine;

public class GamesManagerAuthoring : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject enemyPrefab;
    public class Baker : Baker<GamesManagerAuthoring>
    {
        public override void Bake(GamesManagerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            //将GamesManagerAuthoring上的预制体Baker成Entity，并存储在GameConfigData数据组件中
            GameConfigData gameConfigData = new()
            {
                bulletPortotype = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                enemyPortotype = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic)
            };
            AddComponent(entity, gameConfigData);
        }
    }
}