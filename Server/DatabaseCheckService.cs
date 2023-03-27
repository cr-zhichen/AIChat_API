using System.Diagnostics.CodeAnalysis;
using ChatGPT_API.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatGPT_API.Server;

public class DatabaseCheckService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseCheckService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // 在这里执行您想要在服务启动时进行的操作
        Console.WriteLine("DatabaseCheckService Start");

        ScheduledTask._action.Add("RefreshUser",
            () =>
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<DbLinkContext>();

                    //删除过期的游客
                    context.User.RemoveRange(context.User.Where(x =>
                        x.Grade == 0 && x.ExpireDate < DateTime.Now));
                    //Vip用户过期后降级
                    context.User.Where(x => x.Grade == 2 && x.ExpireDate < DateTime.Now)
                        .ToList().ForEach(x =>
                        {
                            x.Grade = 1;
                            x.ExpireDate = DateTime.Now + TimeSpan.FromDays(1);
                            x.RemainingTimes = 20;
                        });
                    //普通用户每日请求次数重置
                    context.User.Where(x => x.Grade == 1 && x.ExpireDate < DateTime.Now)
                        .ToList().ForEach(x =>
                        {
                            x.ExpireDate = DateTime.Now + TimeSpan.FromDays(1);
                            x.RemainingTimes = 20;
                        });


                    context.SaveChanges();
                }
            });


        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // 在这里执行您想要在服务停止时进行的操作

        return Task.CompletedTask;
    }
}