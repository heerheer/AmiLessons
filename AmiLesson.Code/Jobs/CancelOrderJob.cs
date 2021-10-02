using System;
using System.Threading.Tasks;
using Quartz;

namespace AmiLesson.Code.Jobs
{
    public class CancelOrderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var list = BathRoomConfigUtil.GetAllAutoOrderUser();
            foreach (var user in list)
            {
                var helper = new BathroomOrderHelper();
                await helper.Login(user.user, user.pwd);
                var orders = await helper.GetOrders();
                orders = orders.FindAll(x => x.status is "0");
                foreach (var x in orders)
                {
                    var result = await helper.CancelOrder(x.id);
                    Console.WriteLine($"[{user.user}]预约{x.period}取消:{result}");
                }
            }
        }
    }
}