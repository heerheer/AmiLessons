using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AmiLesson.Code
{
    public class YouKnow
    {
        public static List<string> Datas = new()
        {
            "白袜和黑袜选一个，赫尔肯定选白袜。",
            "布丁是很好吃的东西！",
            "你知道吗数据库在建中!",
            "课表Bot似乎是个私人Bot",
            "为什么教务系统不给API???",
            "救命，救命，不会真有人喜欢白袜眼镜络腮胡吧",
            "GXW啥也不会，asp都要我教他",
            "啊...计量学要好好学哦！",
            "小喵出击！",
            "致命冲击！",
            "伊泽瑞尔是个可以刷新技能CD的ADC！",
            "赛鸽，企划中!",
            "NWY X WZZ 不生孩子很难收场",
            "WZZ什么时候才能干一次NWY...",
            "赫尔老腐男了。"
        };

        public static string Get()
        {
            return Datas[new Random().Next(0, Datas.Count - 1)];
        }
    }
}