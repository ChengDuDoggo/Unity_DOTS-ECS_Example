using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

//JobSystem适用于处理小而多的多线程任务,例如我要控制十万个物体的移动,每个物体的移动计算量很小,但是数量很多
//不适用于长的多线程任务,例如对于一个网络连接多线程,常常需要整个游戏生命周期去执行监听的多线程任务,不适用于JobSystem
public class TestJobSystem : MonoBehaviour
{
    private MyJobFor myJobFor;
    //打破常识:在Unity.Collections库中的值类型数据结构,相当于指针操作器,虽然它们都是值类型,但是在被传递给其他函数中并被改变后
    //它们自己的值也会被改变,并没有像传统值类型是纯赋值传递,传递后的值于自己再无关联
    private NativeArray<int> nums;
    private JobHandle jobHandle;
    private void Start()
    {
        //Thread.CurrentThread.ManagedThreadId:输出当前线程ID
        Debug.Log("主线程:" + Thread.CurrentThread.ManagedThreadId);
        MyJob myJob = new();
        //Run():在主线程中执行
        myJob.Run();
        //Schedule():在工作线程中执行(异步)
        myJob.Schedule();

        nums = new NativeArray<int>(100, Allocator.Persistent);
        myJobFor = new() { nums = nums, jobID = 0 };
        //myJobFor.Run(100);//在主线程中执行100次
        //myJobFor.Schedule(100, default);//在同一个工作线程中执行100次
        jobHandle = myJobFor.ScheduleParallel(100, 10, default);//每10个任务为1组,每组为1个工作线程,并行执行10组任务
        //方式一
        //jobHandle.Complete();//等待Job执行完成才会执行之后代码(阻塞主线程)
        //for (int i = 0; i < nums.Length; i++)
        //{
        //    Debug.Log(nums[i]);
        //}
        //nums.Dispose();//在Job执行完后释放内存,如果Job还没执行完就释放内存会报错

        //Job依赖
        NativeArray<int> nums1 = new(10, Allocator.Persistent);
        MyJobFor job2 = new() { nums = nums1, jobID = 1 };
        job2.ScheduleParallel(10, 10, jobHandle);//jobHandle为job2的依赖,必须等job1执行完才能执行job2
    }
    private void Update()
    {
        //方式二
        if (jobHandle.IsCompleted && jobHandle != default)
        {
            jobHandle.Complete();//即便Job已经是完成状态,也需要调用Complete函数来确保Job的正确结束和内存释放
            for (int i = 0; i < nums.Length; i++)
            {
                Debug.Log(nums[i]);
            }
            nums.Dispose();
            jobHandle = default;
        }
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
//IJobFor:循环处理多个任务
public struct MyJobFor : IJobFor
{
    public int jobID;
    public NativeArray<int> nums;
    public void Execute(int index)
    {
        nums[index] = index;
        Debug.Log("myJobFor线程:" + Thread.CurrentThread.ManagedThreadId + "-index:" + index);
        Debug.Log(jobID + ":" + index);
    }
}