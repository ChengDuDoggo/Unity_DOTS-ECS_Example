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
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}