using System;
using System.IO;
using System.Linq;
using System.Text;
using Amiable.SDK.Enum;
using Amiable.SDK.EventArgs;
using Amiable.SDK.Interface;
using Amiable.SDK.Tool.HIni;
using Quartz;

namespace AmiLesson.Code
{
    public class GroupEvent : IPluginEvent
    {
        public AmiableEventType EventType => AmiableEventType.Group;

        public void Process(AmiableEventArgs e)
        {
            var ee = e.AsMessageEventArgs();
            var message = ee.RawMessage.Replace("啥", "什么");

            if (message is "课表bot" or "课表bot帮助")
            {
                var sb = new StringBuilder();
                sb.AppendLine($"这里是Ami课表Bot~目前是赫尔的专属Bot");
                sb.AppendLine($"> 命令");
                sb.AppendLine($"今天有啥课 —— 查看今天上什么课");
                sb.AppendLine($"明天有啥课 —— 查看明天上什么课");
                sb.AppendLine($"待会儿上啥课 —— 查看明天上什么课");
                sb.AppendLine($"我也要用课表bot —— 查看如何为课表bot添加(导入数据)");

                ee.SendMessage(sb.ToString());
            }

            var msgList = new[] { "今天有什么课", "明天有什么课", "明天有什么课2", "待会儿上什么课", "下一节课", "我也要用课表bot", "测试" };

            if (!msgList.ToList().Exists(x => message.Contains(x)))
            {
                return;
            }

            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Lessons", $"{ee.UserId}.json"));
            if (file.Directory != null && file.Directory.Exists is false)
                file.Directory.Create();
            if (file.Exists is false)
            {
                return;
            }

            if (message == "今天有什么课")
            {
                var ana = new Analyzer(file.FullName);
                var sb = new StringBuilder();
                var lessonInfos = ana.GetTodayLessonInfos(DateTime.Now);
                sb.AppendLine($"今天有{lessonInfos.Count}节课!");
                sb.AppendLine($"分别是:");
                lessonInfos.ForEach(x =>
                {
                    sb.AppendLine($"第{x.StartTime}-{x.EndTime}节:");
                    sb.AppendLine($"{x.Name} - {x.Teacher}");
                    sb.AppendLine($"Location: {x.Where} - {x.Room}");
                });
                sb.AppendLine($"请好好听课~");

                ee.SendMessage(sb.ToString());
            }

            if (message == "明天有什么课")
            {
                var ana = new Analyzer(file.FullName);
                var sb = new StringBuilder();
                var lessonInfos = ana.GetTodayLessonInfos(DateTime.Today.AddDays(1));
                sb.AppendLine($"明天有{lessonInfos.Count}节课!");
                sb.AppendLine($"分别是:");
                lessonInfos.ForEach(x =>
                {
                    sb.AppendLine($"第{x.StartTime}-{x.EndTime}节:");
                    sb.AppendLine($"{x.Name} - {x.Teacher}");
                });
                sb.AppendLine($"明天也请请好好听课哦~");

                ee.SendMessage(sb.ToString());
            }

            if (message == "明天有什么课2")
            {
                SendImageMessage(DateTime.Today.AddDays(1), ee);
            }

            if (message == "今天有什么课2")
            {
                SendImageMessage(DateTime.Today, ee);
            }

            if (message is "待会儿上什么课" or "下一节课")
            {
                var ana = new Analyzer(file.FullName);
                var sb = new StringBuilder();
                var lessonInfo = ana.GetNearLessons();

                if (lessonInfo == null)
                {
                    ee.SendMessage("已经没课啦~");
                    return;
                }

                sb.AppendLine("待会儿你要上....!!");
                sb.AppendLine($"课程:{lessonInfo.Name}");
                sb.AppendLine($"授课老师:{lessonInfo.Teacher}");
                sb.AppendLine($"课程地点:{lessonInfo.Where}");
                sb.AppendLine($"课程教室:{lessonInfo.Room}");

                ee.SendMessage(sb.ToString());
            }

            if (message is "我也要用课表bot")
            {
                var sb = new StringBuilder();
                sb.AppendLine($"前面的区域，以后再来探索吧~");

                ee.SendMessage(sb.ToString());
            }

            if (message == "测试")
            {
                //TriggerUtils
            }
        }

        public void SendImageMessage(DateTime date, AmiableMessageEventArgs ee)
        {
            ee.SendMessage($"噇暮正在祈祷中...");

            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Lessons", $"{ee.UserId}.json"));


            var ana = new Analyzer(file.FullName);
            var lessonInfos = ana.GetTodayLessonInfos(date);


            var drawer = new Drawer(lessonInfos);

            var nick = ee.ApiWrapper.GetNick(ee.UserId.ToString());

            var dayHello = "";
            switch (DateTime.Now.TimeOfDay.Hours)
            {
                case <9:
                    dayHello = "早上好";
                    break;
                case < 12:
                    dayHello = "上午好";
                    break;
                case <19:
                    dayHello = "下午好";
                    break;
                case <23:
                    dayHello = "晚上好";
                    break;
                default:
                    dayHello = "被熬夜啦~";
                    break;
            }

            var mainColor = "62BAAC";
            var iniFileInfo =
                new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Lessons", "backs", $"{ee.UserId}.ini"));
            if (iniFileInfo.Exists)
            {
                IniObject iniObject = new IniObject(iniFileInfo.FullName);
                iniObject.Load();
                var value = iniObject["skin"]["主色"];
                mainColor = string.IsNullOrEmpty(value) ? mainColor : value;
            }

            drawer.MainColor = mainColor;

            using (var bitmap = drawer.Draw($"{dayHello},{nick}", ee.UserId))
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Ami", "Lessons", "temps",
                    $"{ee.UserId}_temp.jpg");
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                bitmap.Save(path);
                ee.SendMessage($"[pic={path}]");
            }
        }
    }
}