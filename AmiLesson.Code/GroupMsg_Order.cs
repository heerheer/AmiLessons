using Amiable.SDK.Enum;
using Amiable.SDK.EventArgs;
using Amiable.SDK.Interface;
using Amiable.SDK.Tool.HIni;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiLesson.Code
{
    public class GroupMsg_Order : IPluginEvent
    {
        public AmiableEventType EventType => AmiableEventType.Group;

        public async void Process(AmiableEventArgs e)
        {
            var ee = e.AsMessageEventArgs();
            var message = ee.RawMessage;


            if (message == "洗澡#我的二维码")
            {
                var account = BathRoomConfigUtil.GetAccount(ee.UserId);
                var sb = new StringBuilder();
                if (account.user == "")
                {
                    sb.AppendLine($"[@{ee.UserId}]");
                    sb.AppendLine($"没有你的信息，联系赛马之神来建立档案");
                    ee.SendMessage(sb.ToString());
                    return;
                }
                var helper = new BathroomOrderHelper();
                await helper.Login(account.user, account.pwd);
                var url = await helper.GetQrCode();

                sb.AppendLine($"[@{ee.UserId}]");
                sb.AppendLine($"你的QR码已生成");
                //sb.AppendLine($"请私聊获取~(如果没有噇暮好友记得加噇暮哦~)");
                sb.AppendLine($"{url}");
                ee.SendMessage(sb.ToString());

                //ee.ApiWrapper.SendPrivateMessage(ee.UserId.ToString(), url);
            }
            if (message == "洗澡#检查账户")
            {
                var account = BathRoomConfigUtil.GetAccount(ee.UserId);
                var sb = new StringBuilder();
                if (account.user == "")
                {
                    sb.AppendLine($"[@{ee.UserId}]");
                    sb.AppendLine($"没有你的信息，联系赛马之神来建立档案");
                    ee.SendMessage(sb.ToString());
                    return;
                }
                var helper = new BathroomOrderHelper();
                await helper.Login(account.user, account.pwd);
                sb.AppendLine($"[@{ee.UserId}]");
                sb.AppendLine($"账户登入情况:{(helper.Logined ? "成功" : "失败")}");
                sb.AppendLine($"自动预约开启状态:{(BathRoomConfigUtil.GetEnable(ee.UserId,"自动预约") ? "开启" : "关闭")}");
                ee.SendMessage(sb.ToString());
            }
            if (message == "洗澡#我的预约")
            {
                var account = BathRoomConfigUtil.GetAccount(ee.UserId);
                var sb = new StringBuilder();
                if (account.user == "")
                {
                    sb.AppendLine($"[@{ee.UserId}]");
                    sb.AppendLine($"没有你的信息，联系赛马之神来建立档案");
                    ee.SendMessage(sb.ToString());
                    return;
                }
                var helper = new BathroomOrderHelper();
                await helper.Login(account.user, account.pwd);
                var list = await helper.GetOrders();
                sb.AppendLine($"[@{ee.UserId}]");
                list.ForEach(x =>
                {
                    var statusStr = x.status switch
                    {
                        "0" => "已预约",
                        "1" => "已扫码进入",
                        "2" => "已完成",
                        "3" => "已超时",
                        "4" => "已归档",
                    };
                    sb.AppendLine($"[{x.bathRoomName}]->[{x.period}]->状态{statusStr}");

                });
                ee.SendMessage(sb.ToString());
            }
            if (message == "洗澡#启用自动预约")
            {
                var account = BathRoomConfigUtil.GetAccount(ee.UserId);
                var sb = new StringBuilder();
                if (account.user == "")
                {
                    sb.AppendLine($"[@{ee.UserId}]");
                    sb.AppendLine($"没有你的信息，联系赛马之神来建立档案");
                    ee.SendMessage(sb.ToString());
                    return;
                }
                BathRoomConfigUtil.SetAutoOrderEnable(ee.UserId);
                sb.AppendLine($"[@{ee.UserId}]");
                sb.AppendLine($"已开启自动预约");
                ee.SendMessage(sb.ToString());

            }
            if (message == "洗澡#关闭自动预约")
            {
                var account = BathRoomConfigUtil.GetAccount(ee.UserId);
                var sb = new StringBuilder();
                if (account.user == "")
                {
                    sb.AppendLine($"[@{ee.UserId}]");
                    sb.AppendLine($"没有你的信息，联系赛马之神来建立档案");
                    ee.SendMessage(sb.ToString());
                    return;
                }
                BathRoomConfigUtil.SetAutoOrderEnable(ee.UserId, false);
                sb.AppendLine($"[@{ee.UserId}]");
                sb.AppendLine($"已关闭自动预约");
                ee.SendMessage(sb.ToString());

            }

            if (message == "洗澡#手动预约")
            {
            }
        }
    }

    public class BathRoomConfigUtil
    {
        public static (string user, string pwd) GetAccount(long userId)
        {
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Bathroom", $"data.ini"));
            if (file.Directory.Exists is false)
                file.Directory.Create();
            if (file.Exists is false)
                file.Create().Close();
            IniObject ini = new(file.FullName);
            ini.Load();
            return (ini[userId.ToString()]["user"], ini[userId.ToString()]["pwd"]);
        }

        public static void SetAutoOrderEnable(long userId, bool enable = true)
        {
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Bathroom", $"data.ini"));
            if (file.Directory.Exists is false)
                file.Directory.Create();
            if (file.Exists is false)
                file.Create().Close();
            IniObject ini = new(file.FullName);
            ini.Load();
            ini[userId.ToString()]["自动预约"] = enable.ToString();
            ini.Save();
        }

        public static bool GetEnable(long userId,string key)
        {
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Bathroom", $"data.ini"));

            IniObject ini = new(file.FullName);
            ini.Load();
            return bool.Parse(ini[userId.ToString()][key]);
            
        }
        public static List<(long userId,string user,string pwd)> GetAllAutoOrderUser()
        {
            List<(long userId, string user, string pwd)> list = new();
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Bathroom", $"data.ini"));

            IniObject ini = new(file.FullName);
            ini.Load();
            ini.Sections.ForEach(x => {
                list.Add((long.Parse(x.SectionName), x["user"], x["pwd"]));
            });

            return list;

        }
    }
}
