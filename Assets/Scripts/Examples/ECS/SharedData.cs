using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public static class SharedData
{
    public static readonly SharedStatic<Entity> singtonEntity = SharedStatic<Entity>.GetOrCreate<KeyClass1>();
    public static readonly SharedStatic<SpawnEnemySharedData> spawnEnemySharedData = SharedStatic<SpawnEnemySharedData>.GetOrCreate<SpawnEnemySharedData>();
    public static readonly SharedStatic<float2> playerPos = SharedStatic<float2>.GetOrCreate<KeyClass2>();
    public struct KeyClass1 { }
    public struct KeyClass2 { }
}
public struct SpawnEnemySharedData
{
    public int deadCounter;
    public float spawnInterval;
    public int spawnCount;
}