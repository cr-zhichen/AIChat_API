using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

/// <summary>
/// 定时任务
/// </summary>
public static class ScheduledTask
{
    public static Dictionary<string, Action> _action = new Dictionary<string, Action>();
    public static int _intervalSecond;

    /// <summary>
    /// 开始任务
    /// </summary>
    public static void Start()
    {
        //新建一个线程用来执行定时任务
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                foreach (var item in _action)
                {
                    item.Value();
                }

                //每隔一段时间执行一次
                Task.Delay(_intervalSecond * 1000).Wait();
            }
        });
    }
}