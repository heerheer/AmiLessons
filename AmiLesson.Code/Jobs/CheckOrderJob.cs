using System;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace AmiLesson.Code.Jobs
{
    public class CheckOrderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var list = BathRoomConfigUtil.GetAllAutoOrderUser();

            foreach (var tuple in list)
            {
                var helper = new BathroomOrderHelper();
                await helper.Login(tuple.user, tuple.pwd);
                var orders = await helper.GetOrders();
                /*var nearOrders = orders.FindAll(x =>
                    DateTime.Now - DateTimeOffset.FromUnixTimeMilliseconds(x.periodEndTime).ToLocalTime() <
                    TimeSpan.FromMinutes(1));*/
                orders = orders.FindAll(x => x.status is "0");
                if (orders.Count == 0)
                    continue;
                var sb = new StringBuilder();
                sb.AppendLine($"用户:{tuple.user}");
                sb.AppendLine($"当前未扫码进入的预约数量:{orders.Count}");
                orders.ForEach(x =>
                {
                    sb.AppendLine($"[{x.bathRoomName}]=>[{x.period}]");

                });
                EnableEvent.Wrapper.SetData(new()
                    { Robot = 3324288929 });
                EnableEvent.Wrapper.SendGroupMessage("788599289", sb.ToString());

                await Task.Delay(10000);
            }
        }
    }

    public class TestJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"现在是{DateTime.Now.ToShortTimeString()}");
        }
    }
}