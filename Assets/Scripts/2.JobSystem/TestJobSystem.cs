using System.Threading;
using Unity.Jobs;
using UnityEngine;

//JobSystem适用于处理小而多的多线程任务,例如我要控制十万个物体的移动,每个物体的移动计算量很小,但是数量很多
//不适用于长的多线程任务,例如对于一个网络连接多线程,常常需要整个游戏生命周期去执行监听的多线程任务,不适用于JobSystem
public class TestJobSystem : MonoBehaviour
{
    private void Start()
    {
        //Thread.CurrentThread.ManagedThreadId:输出当前线程ID
        Debug.Log("主线程:" + Thread.CurrentThread.ManagedThreadId);
        MyJob myJob = new();
        //Run():在主线程中执行
        myJob.Run();
        //Schedule():在工作线程中执行(异步)
        myJob.Schedule();
    }
}
//IJob:普通的单任务
public struct MyJob : IJob
{
    //Job实际执行的函数
    public readonly void Execute()
    {
        Debug.Log("myJob线程:" + Thread.CurrentThread.ManagedThreadId);
    }
}