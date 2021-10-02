using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amiable.SDK.Enum;
using Amiable.SDK.EventArgs;
using Amiable.SDK.Interface;
using Amiable.SDK.Wrapper;
using AmiLesson.Code.Jobs;
using Quartz;
using Quartz.Impl;
using static AmiLesson.Code.Jobs.OrderJob;

namespace AmiLesson.Code
{
    public class EnableEvent : IPluginEvent
    {
        public static IApiWrapper Wrapper;
        public static StdSchedulerFactory SchedulerFactory;
        public static IScheduler Scheduler;
        public AmiableEventType EventType => AmiableEventType.PluginEnable;

        public void Process(AmiableEventArgs e)
        {
            Wrapper = e.ApiWrapper;
            SchedulerFactory = new StdSchedulerFactory();
            Scheduler = SchedulerFactory.GetScheduler().Result;
            Scheduler.Start();

            var job = JobBuilder.Create<TopJob>().Build();
            // ReSharper disable once SuspiciousTypeConversion.Global
            var trigger = TriggerBuilder.Create()
                .WithIdentity("Trigger_Lessons")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(21, 0))
                .Build();
            Scheduler.ScheduleJob(job, trigger);

            Console.WriteLine("[Ami课表助手]调度器启用完成");
            Console.WriteLine("[Ami课表助手]每天21:00分会在群788599289发送提醒");

            var orderJob = JobBuilder.Create<OrderJob>().Build();
            var trigger2 = TriggerBuilder.Create()
                .WithIdentity("Trigger_Order")
                .WithCronSchedule("0 0 7 * * ? *")
                .Build();
            Scheduler.ScheduleJob(orderJob, trigger2);
            
            var preorderJob = JobBuilder.Create<PreOrderJob>().Build();
            var preorderTrigger = TriggerBuilder.Create()
                .WithIdentity("Trigger_PreOrder")
                .WithCronSchedule("0 50 6 * * ? *")
                .Build();
            Scheduler.ScheduleJob(preorderJob, preorderTrigger);
            
            
            var autoCancelOrderJob = JobBuilder.Create<CancelOrderJob>().Build();
            // ReSharper disable once SuspiciousTypeConversion.Global
            var autoCancelOrderTrigeer = TriggerBuilder.Create()
                .WithIdentity("Trigger_CancelOrder")
                .WithCronSchedule("0 40 21 * * ? *")
                .Build();
            Scheduler.ScheduleJob(autoCancelOrderJob, autoCancelOrderTrigeer);

            Console.WriteLine("[Ami自动预约助手]调度器启用完成");
        }
    }


    public class TopJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Factory.StartNew(async () =>
            {
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}][课表Bot]定时任务...");
                var qqs = new long[] { 2356045563, 1767407822, 1443116043 };
                foreach (var qq in qqs)
                {
                    var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Lessons", $"{qq}.json"));
                    var ana = new Analyzer(file.FullName);
                    var sb = new StringBuilder();
                    sb.AppendLine($"[@{qq}]");
                    var lessonInfos = ana.GetTodayLessonInfos(DateTime.Today.AddDays(1));
                    if (lessonInfos.Count == 0)
                    {
                        sb.AppendLine($"明天没有课~可以去图书馆或者空教室自习哦~");
                    }
                    else
                    {
                        sb.AppendLine($"明天有{lessonInfos.Count}节课!");
                        sb.AppendLine($"分别是:");
                        lessonInfos.ForEach(x =>
                        {
                            sb.AppendLine($"第{x.StartTime}-{x.EndTime}节:");
                            sb.AppendLine($"{x.Name} - {x.Teacher}");
                            sb.AppendLine($"Location: {x.Where} - {x.Room}");
                        });
                        sb.AppendLine($"明天也请请好好听课哦~");
                    }

                    EnableEvent.Wrapper.SetData(new()
                        { Robot = 3324288929 });
                    EnableEvent.Wrapper.SendGroupMessage("788599289", sb.ToString());
                    await Task.Delay(8000);
                }
            });
        }
    }
}