using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public Vector3 scale = Vector3.one;
    public float moveSpeed = 4.0f;
    public class EnemyBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RendererSortTag>(entity);
            SetComponentEnabled<RendererSortTag>(entity, true);
        }
    }
}