using Unity.Entities;
using UnityEngine;

public class CarrotBulletAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float destroyTime;
    public class Baker : Baker<CarrotBulletAuthoring>
    {
        public override void Bake(CarrotBulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RendererSortTag>(entity);
            AddComponent<BulletData>(entity);
            SetComponentEnabled<RendererSortTag>(entity, true);
        }
    }
}