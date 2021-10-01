using System;
using System.Collections.Generic;
using System.IO;
using Amiable.SDK.Tool.HIni;

namespace AmiLesson.Code
{
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

        public static bool GetEnable(long userId, string key)
        {
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Bathroom", $"data.ini"));

            IniObject ini = new(file.FullName);
            ini.Load();
            return bool.Parse(ini[userId.ToString()][key]);
        }

        public static List<(long userId, string user, string pwd)> GetAllAutoOrderUser()
        {
            List<(long userId, string user, string pwd)> list = new();
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Bathroom", $"data.ini"));

            IniObject ini = new(file.FullName);
            ini.Load();
            ini.Sections.ForEach(x => { list.Add((long.Parse(x.SectionName), x["user"], x["pwd"])); });

            return list;
        }
    }
}