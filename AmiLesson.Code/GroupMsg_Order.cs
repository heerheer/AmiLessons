using Amiable.SDK.Enum;
using Amiable.SDK.EventArgs;
using Amiable.SDK.Interface;
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
            if (message.StartsWith("洗澡#账号状态"))
            {
                if (ee.UserId != 1767407822)
                    return;
                var account = message.Substring(7).Replace("[@", "").Replace("]", "").Trim();

                CheckAccount(long.Parse(account), ee);
            }

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
                CheckAccount(ee.UserId, ee);
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
                var list1 = await helper.GetList();
                var list = await helper.GetRoomList(list1.Find(x => x.name == "南区男浴室")?.id ?? 0);
                sb.AppendLine($"[@{ee.UserId}]");
                list.Take(6).ToList().ForEach(x => { sb.AppendLine($"{x.period} -> id:{x.id}"); });
                sb.AppendLine($"输入洗澡#手动预约+id来手动预约");

                ee.SendMessage(sb.ToString());

                return;
            }

            if (message.StartsWith("洗澡#手动预约"))
            {
                var id = message.Substring(7).Trim();
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
                var result = await helper.Order(int.Parse(id));

                sb.AppendLine($"[预约成功] -> {result.message}");
                sb.AppendLine($"你可以发送 洗澡#我的预约 来查看你的预约");

                ee.SendMessage(sb.ToString());
            }
        }

        public async void CheckAccount(long userId, AmiableMessageEventArgs ee)
        {
            var account = BathRoomConfigUtil.GetAccount(userId);
            var sb = new StringBuilder();

            if (account.user == "")
            {
                sb.AppendLine($"[@{userId}]");
                sb.AppendLine($"没有你的信息，联系赛马之神来建立档案");
                ee.SendMessage(sb.ToString());
                return;
            }

            var helper = new BathroomOrderHelper();
            await helper.Login(account.user, account.pwd);
            sb.AppendLine($"[@{userId}]");
            sb.AppendLine($"账户登入情况:{(helper.Logined ? "成功" : "失败")}");
            sb.AppendLine($"自动预约开启状态:{(BathRoomConfigUtil.GetEnable(userId, "自动预约") ? "开启" : "关闭")}");
            ee.SendMessage(sb.ToString());
        }
    }
}