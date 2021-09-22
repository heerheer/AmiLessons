using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amiable.SDK;
using Quartz;

namespace AmiLesson.Code.Jobs
{
    public class OrderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.Now;
            List<Task> tasks = new();
            var list = PreOrderJob.UserWithAccessToken;
            var sb = new StringBuilder();
            InnerLog($"开始执行计划...{now.ToShortTimeString()}", sb);
            foreach (var item in list)
            {
                var task = new Task(() =>
                {
                    var helper = new BathroomOrderHelper();
                    var orderResult = helper.Order(558, item.LoginId, item.Token).Result;

                    if (orderResult.succeed)
                        InnerLog($"[{item.User}]预约成功->{orderResult.message}", sb);
                    else
                        InnerLog($"[{item.User}]预约失败!", sb);
                });

                task.Start();
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            InnerLog($"完成用时{(DateTime.Now - now).TotalSeconds}秒", sb);

            await Task.Delay(10000);
            EnableEvent.Wrapper.SetData(new()
                { Robot = 3324288929 });
            AppService.Instance.Log(sb.ToString());
            EnableEvent.Wrapper.SendGroupMessage("788599289", sb.ToString());
        }

        public void InnerLog(string str, StringBuilder sb)
        {
            sb.AppendLine(str);
            Console.WriteLine(str);
        }
    }
}