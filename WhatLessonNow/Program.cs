using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AmiLesson.Code;
using Quartz;
using Quartz.Impl;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using DingtalkChatbotSdk.Models;
using System.Linq;

namespace WhatLessonNow
{
    internal class Program
    {
        public class TipJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                return Task.Factory.StartNew(() => { Console.WriteLine("Hello Quartz.Net"); });
            }
        }

        public static async Task Main(string[] args)
        {
            var now = DateTime.Now;
            Console.WriteLine($"开始执行计划...{DateTime.Now}");
            List<Task> tasks = new();
            var list = new[] { ("20191953", "800518"), ("20191881", "191881"), ("20191951", "566710"), ("20191954", "191954"), ("20191955", "191955")/*, ("20191952", "191952")*/ };
            var helper = new BathroomOrderHelper();

            await helper.Login(list[0].Item1, list[0].Item2);
            var orders = await helper.GetOrders();
            orders.ForEach(x => {
                Console.WriteLine($"{x.bathRoomName}-{x.period}-{DateTimeOffset.FromUnixTimeMilliseconds(x.createTime).ToLocalTime()}");
            });

            {

                foreach (var item in list)
                {
                    var task = new Task(() =>
                    {
                        var helper = new BathroomOrderHelper();
                        helper.Login(item.Item1, item.Item2).Wait();

                        if (helper.Logined)
                        {
                            var list = helper.GetList().Result;
                            var list2 = helper.GetRoomList(list.Find(x => x.name == "南区男浴室").id).Result;

                        }

                    //var id = list2.Last(x => x.remain >= 1).id;
                    //Console.WriteLine($"正在为{item.Item1}预约{id}");
                    //var result = helper.Order(id).Result;
                    //if (result.Item1)
                    //{
                    //    Console.WriteLine($"[{item.Item1}]预约成功->{result.Item2}");
                    //}
                    //else
                    //{
                    //    Console.WriteLine($"[{item.Item1}]预约失败!");

                    //}

                });

                    tasks.Add(task);
                    task.Start();
                }
                Task.WaitAll(tasks.ToArray());
                Console.WriteLine($"{tasks.Count}个账户计划执行完成...总计{(DateTime.Now - now).TotalMilliseconds}毫秒");

            }
            //await helper.Login("20191953", "800518");
            Console.ReadLine();
        }

        public async static Task Main3(string[] args)
        {
            var md = $"![xxx](http://server1.heerdev.top/A/1767407822_temp.jpg)";

            using (HttpClient client = new HttpClient())
            {
                var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var secret = "SECdca2d4a7a2b197173062eaa17b1ff5a2ae44065d30bb933391667ca9f6a20537";
                var stringToSign = timestamp + "\n" + secret;

                string sign;
                using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    var signByte = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                    sign = Convert.ToBase64String(signByte);
                }

                var url =
                    @"https://oapi.dingtalk.com/robot/send?access_token=c1a8a06ec01f79c3bfe56ba54263256b60cb486c8c99e38eceeb53cb8971a82b"
                    + $"&timestamp={timestamp}"
                    + $"&sign={sign}";
                var content = new StringContent($"{{\"msgtype\": \"markdown\",\"markdown\": {{\"title\":\"噇暮的头像\",\"text\":\"{md}\"}}}}");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var str = await client.PostAsync(url, content);
                Console.WriteLine("POST完成");
                Console.WriteLine(await str.Content.ReadAsStringAsync());
            }

            /*var list = new Analyzer(@"E:\RiderProjects\WhatLessonNow\publish\1767407822.json").GetTodayLessonInfos(DateTime.Now);
            using (var bitmap = new Drawer(list){MainColor = "7977ED"}.Draw("傻逼", 1767407822))
            {
                bitmap.Save(@"E:\RiderProjects\WhatLessonNow\publish\temp.jpg");
                Process.Start("explorer.exe", @"E:\RiderProjects\WhatLessonNow\publish\temp.jpg");
            }*/
        }

        public static void Main2(string[] args)
        {
            var datas = new List<LessonInfo>()
            {
                new()
                {
                    StartTime = 1,
                    EndTime = 2,
                    Where = "4号楼",
                    Room = "4-202",
                    Teacher = "章聂3",
                    Name = "章氏抉择的过去与未来",
                    StartWeek = 10,
                    EndWeek = 12,
                    Mode = 0,
                    Week = 1
                },
                new()
                {
                    StartTime = 4,
                    EndTime = 5,
                    Where = "3号楼",
                    Room = "3-102",
                    Teacher = "西林",
                    Name = "回忆学",
                    StartWeek = 1,
                    EndWeek = 16,
                    Mode = 0,
                    Week = 1
                }
            };
            var path = @"E:\RiderProjects\WhatLessonNow\1.png";
            using (Bitmap bitmap = new Bitmap(800, datas.Count * 260))
            {
                Graphics graphics = Graphics.FromImage(bitmap);
                for (int i = 0; i < datas.Count; i++)
                {
                    var data = datas[i];

                    var rect = new Rectangle(0, 260 * i, 800, 260);

                    using (var stream = File.Open(path, FileMode.Open))
                    {
                        using (var image = Image.FromStream(stream))
                        {
                            graphics.DrawImage(image, rect);
                        }
                    }

                    var brush = new SolidBrush(ColorTranslator.FromHtml("#62BAAC"));
                    var nameFont = new Font("微软雅黑", 36, GraphicsUnit.Pixel);
                    var font = new Font("微软雅黑", 22, GraphicsUnit.Pixel);
                    graphics.DrawString(data.Name, nameFont, brush, 157, 49 + (i * 260));


                    graphics.DrawString(
                        $"{Analyzer.TimeList[data.StartTime - 1].Item1:hh\\:mm}-{Analyzer.TimeList[data.EndTime - 1].Item2:hh\\:mm}",
                        font, brush, 146, 137 + (i * 260));
                    graphics.DrawString($"第{data.StartTime}-{data.EndTime}节", font, Brushes.White, 680, 54 + (i * 260));
                    graphics.DrawString($"{data.Where}", font, brush, 142, 197 + (i * 260));
                    graphics.DrawString($"{data.Room}", font, brush, 371, 197 + (i * 260));
                    graphics.DrawString($"{data.Teacher}", font, brush, 400, 137 + (i * 260));

                    var tip = "你知道吗:这里可以写你知道吗";
                    var tipFont = new Font("微软雅黑", 16, GraphicsUnit.Pixel);

                    var tipRect = new Rectangle(535, 168 + (i * 260), 219, 60);
                    graphics.DrawString(tip, tipFont, brush, tipRect, new StringFormat()
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Near
                    });
                }

                graphics.Save();
                bitmap.Save("temp.jpg");
                Process.Start("explorer.exe", Path.Combine(AppContext.BaseDirectory, "temp.jpg"));
            }
        }
    }
}