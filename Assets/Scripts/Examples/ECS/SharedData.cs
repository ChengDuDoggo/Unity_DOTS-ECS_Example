using Unity.Burst;
using Unity.Entities;

public static class SharedData
{
    public static readonly SharedStatic<Entity> singtonEntity = SharedStatic<Entity>.GetOrCreate<KeyClass1>();
    public struct KeyClass1 { }
}