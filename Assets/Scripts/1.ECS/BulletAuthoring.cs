using Unity.Entities;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    public float moveSpeed = 10;
    public class BulletBaker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            Entity bulletEntity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<MoveData>(bulletEntity, new MoveData()
            {
                moveSpeed = authoring.moveSpeed
            });
        }
    }
}