using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace AmiLesson.Code
{
    public class BathroomOrderHelper
    {
        private long timestamp;
        public int LoginId { get; set; }
        public string AccessToken { get; set; }

        public bool Logined { get; set; } = false;
        public string user;


        /// <summary>
        /// 分类列表
        /// </summary>
        private List<BathRoomListItem> AllRooms;

        public BathroomOrderHelper()
        {
            timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(); //初始化时间戳
            //Console.WriteLine("时间戳为" + timestamp);
        }

        public async Task Login(string user, string pwd)
        {
            this.user = user;
            var url = $"http://ligong.deshineng.com:8082/brmclg/api/logon/login?time={timestamp}";
            var pwd_code = GetMD5(pwd);
            using (HttpClient client = new())
            {
                var json = $"{{\"code\": \"{user}\", \"password\": \"{pwd_code}\"}}";
                StringContent stringContent = new(json);
                stringContent.Headers.ContentType = new("application/json");
                var response = await client.PostAsync(url, stringContent);
                var data = await response.Content.ReadAsStringAsync();

                using (var jdoc = JsonDocument.Parse(data))
                {
                    if (jdoc.RootElement.GetProperty("data").GetRawText() == "{}")
                    {
                        Console.WriteLine($"[{user}]登入失败");
                        Logined = false;
                        return;
                    }

                    LoginId = jdoc.RootElement.GetProperty("data").GetProperty("loginid").GetInt32();
                    AccessToken = jdoc.RootElement.GetProperty("data").GetProperty("token").GetString();
                    Logined = true;
                    Console.WriteLine($"[{user}]登入成功");
                }
            }
        }

        public async Task<List<BathRoomListItem>> GetList()
        {
            if (!Logined)
                return new();
            var url = $"http://ligong.deshineng.com:8082/brmclg/api/bathRoom/listRoom?time={timestamp}";
            StringContent stringContent = new("");
            stringContent.Headers.ContentType = new("application/json");
            AddHeader(stringContent);
            using (HttpClient client = new())
            {
                var response = await client.PostAsync(url, stringContent);
                var data = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(data);
                using (var jdoc = JsonDocument.Parse(data))
                {
                    var text = jdoc.RootElement.GetProperty("data").GetProperty("bathRoomList").GetRawText();

                    AllRooms = JsonSerializer.Deserialize<List<BathRoomListItem>>(text);
                }
            }

            foreach (var room in AllRooms)
            {
                //Console.WriteLine($"[{room.name}]->id:{room.id}");
            }

            return AllRooms;
        }

        public async Task<List<RoomContentItem>> GetRoomList(int id)
        {
            if (!Logined)
                return new();
            List<RoomContentItem> roomContentList;
            var url =
                $"http://ligong.deshineng.com:8082/brmclg/api/bathRoom/listBookStatus?time={timestamp}&bathroomid={id}";
            StringContent stringContent = new("");
            stringContent.Headers.ContentType = new("application/json");
            AddHeader(stringContent);
            using (HttpClient client = new())
            {
                var response = await client.PostAsync(url, stringContent);
                var data = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(data);
                using (var jdoc = JsonDocument.Parse(data))
                {
                    var text = jdoc.RootElement.GetProperty("data").GetProperty("bookStatusList").GetRawText();

                    roomContentList = JsonSerializer.Deserialize<List<RoomContentItem>>(text);

                    roomContentList.ForEach(x =>
                    {
                        //Console.WriteLine($"[{x.period}]当前剩余:{x.remain};id:{x.id}");
                    });
                }
            }

            return roomContentList;
        }


        public static async Task<(bool succeed, string message)> Order(int id, int loginid, string accessToken)
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(); //初始化时间戳
            var url =
                $"http://ligong.deshineng.com:8082/brmclg/api/bathRoom/bookOrder?time={timestamp}&bookstatusid={id}";
            StringContent stringContent = new("");
            stringContent.Headers.ContentType = new("application/json");
            AddHeader(stringContent, loginid, accessToken);
            using (HttpClient client = new())
            {
                var response = await client.PostAsync(url, stringContent);
                var data = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(data);
                using (var jdoc = JsonDocument.Parse(data))
                {
                    var text = jdoc.RootElement.GetProperty("data").GetProperty("succeed").GetString();
                    var listJson = jdoc.RootElement.GetProperty("data").GetProperty("bookOrderList").GetRawText();

                    if (text == "Y")
                    {
                        var order = JsonSerializer.Deserialize<List<OrderListItem>>(listJson)?[0];
                        return (true, $"{order.bathRoomName}->时间{order.period}");
                    }
                    else
                    {
                        return (false, data);
                    }
                }
            }
        }


        public async Task<(bool succeed, string message)> Order(int id)
        {
            return await Order(id, this.LoginId, this.AccessToken);
        }

        public async Task<bool> CancelOrder(int id)
        {
            if (!Logined)
                return false;
            var url =
                $"http://ligong.deshineng.com:8082/brmclg/api/bathRoom/cancelOrder?time={timestamp}&bookorderid={id}";
            StringContent stringContent = new("");
            stringContent.Headers.ContentType = new("application/json");
            AddHeader(stringContent);
            using (HttpClient client = new())
            {
                var response = await client.PostAsync(url, stringContent);
                var data = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(data);
                using (var jdoc = JsonDocument.Parse(data))
                {
                    var text = jdoc.RootElement.GetProperty("data").GetProperty("succeed").GetString();
                    if (text == "Y")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public async Task<string> GetQrCode()
        {
            if (!Logined)
                return "";
            var url = $"http://ligong.deshineng.com:8082/brmclg/api/bathRoom/getQcode?time={timestamp}";
            StringContent stringContent = new("");
            stringContent.Headers.ContentType = new("application/json");
            AddHeader(stringContent);
            using (HttpClient client = new())
            {
                var response = await client.PostAsync(url, stringContent);
                var data = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(data);
                using (var jdoc = JsonDocument.Parse(data))
                {
                    var picName = jdoc.RootElement.GetProperty("data").GetProperty("picName").GetString();
                    var lastChar = picName.Last();

                    var picUrl = $"http://ligong.deshineng.com:8082/brmclg/download/{lastChar}/{picName}.jpg";
                    return picUrl;
                }
            }
        }

        public async Task<List<OrderListItem>> GetOrders()
        {
            if (!Logined)
                return new();
            var url = $"http://ligong.deshineng.com:8082/brmclg/api/bathRoom/getBookOrderList?time={timestamp}";
            StringContent stringContent = new("");
            stringContent.Headers.ContentType = new("application/json");
            AddHeader(stringContent);

            using (HttpClient client = new())
            {
                //添加头
                client.DefaultRequestHeaders.Add("token", AccessToken);
                client.DefaultRequestHeaders.Add("loginid", LoginId.ToString());

                var response = await client.GetAsync(url);

                var data = await response.Content.ReadAsStringAsync();

                //Console.WriteLine(data);
                using (var jdoc = JsonDocument.Parse(data))
                {
                    List<OrderListItem> list = JsonSerializer.Deserialize<List<OrderListItem>>(jdoc.RootElement
                        .GetProperty("data").GetProperty("bookOrderList").GetRawText());

                    return list;
                }
            }
        }

        private static void AddHeader(HttpContent httpContent, int loginId, string accessToken)
        {
            httpContent.Headers.Add("token", accessToken);
            httpContent.Headers.Add("loginid", loginId.ToString());
        }

        private void AddHeader(HttpContent httpContent)
        {
            AddHeader(httpContent, LoginId, AccessToken);
        }

        private string GetMD5(string str)
        {
            MD5CryptoServiceProvider md5 = new();
            var bytValue = Encoding.UTF8.GetBytes(str);
            var bytHash = md5.ComputeHash(bytValue);
            return BitConverter.ToString(bytHash).Replace("-", "").ToLower();
        }


        public class BathRoomListItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int maxBookNum { get; set; }
            public string bookStartTime { get; set; }
            public string openTime { get; set; }
            public string closeTime { get; set; }
            public bool state { get; set; }
            public int currentNum { get; set; }
            public string category { get; set; }
            public object buildingId { get; set; }
        }


        public class RoomContentItem
        {
            public int id { get; set; }
            public int bathRoomId { get; set; }
            public string bathRoomName { get; set; }
            public int maxBookNum { get; set; }
            public int bookNum { get; set; }
            public string period { get; set; }
            public string periodStart { get; set; }
            public string periodEnd { get; set; }
            public bool state { get; set; }
            public string category { get; set; }
            public string busy { get; set; }
            public string workDay { get; set; }
            public int remain { get; set; }
        }

        public class OrderListItem
        {
            public int id { get; set; }
            public int bathRoomId { get; set; }
            public int studentId { get; set; }
            public string status { get; set; }
            public string period { get; set; }
            public int bookStatusId { get; set; }
            public string bathRoomName { get; set; }
            public long periodStartTime { get; set; }
            public long periodEndTime { get; set; }
            public object enterTime { get; set; }
            public object leaveTime { get; set; }
            public int cardNo { get; set; }
            public string orderNo { get; set; }
            public string studentName { get; set; }
            public string category { get; set; }
            public long createTime { get; set; }
            public string enterTimeStr { get; set; }
            public string leaveTimeStr { get; set; }
            public string createTimeStr { get; set; }
        }
    }
}