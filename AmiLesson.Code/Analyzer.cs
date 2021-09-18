using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AmiLesson.Code
{
    public class Analyzer
    {
        public static List<(TimeSpan, TimeSpan)> TimeList = new()
        {
            (TimeSpan.Parse("08:00"), TimeSpan.Parse("08:45")),
            (TimeSpan.Parse("08:55"), TimeSpan.Parse("09:40")),
            (TimeSpan.Parse("10:10"), TimeSpan.Parse("10:55")),
            (TimeSpan.Parse("11:05"), TimeSpan.Parse("11:50")),
            (TimeSpan.Parse("14:00"), TimeSpan.Parse("14:45")),
            (TimeSpan.Parse("14:55"), TimeSpan.Parse("15:40")),
            (TimeSpan.Parse("16:10"), TimeSpan.Parse("16:55")),
            (TimeSpan.Parse("17:05"), TimeSpan.Parse("17:50")),
        };

        private List<LessonInfo> Infos;

        public Analyzer(string path)
        {
            var json = File.ReadAllText(path);
            Infos = JsonSerializer.Deserialize<List<LessonInfo>>(json);
        }

        public LessonInfo GetNearLessons()
        {
            var nowIndex = GetIndexOfNow();
            return GetTodayLessonInfos(DateTime.Today).Find(x => x.StartTime > nowIndex);
        }

        public List<LessonInfo> GetTodayLessonInfos(DateTime dateTime)
        {
            var weekNum = (int)dateTime.DayOfWeek;

            var weekIndex = GetWeekIndex(dateTime);
            var lessonInfos = Infos.Where(x =>
                x.StartWeek <= weekIndex && x.EndWeek >= weekIndex && x.Week == weekNum).ToList();

            foreach (var lesson in lessonInfos.ToArray())
            {
                if (lesson.Mode == 1 && weekIndex % 2 == 0)
                {
                    //Console.WriteLine($"{lesson.Name}因为是单周，但是本周是双周，被删除");
                    lessonInfos.Remove(lesson);
                }

                if (lesson.Mode == 2 && weekIndex % 2 != 0)
                {
                    //Console.WriteLine($"{lesson.Name}因为是双周，但是本周是单周，被删除");

                    lessonInfos.Remove(lesson);
                }
            }

            return lessonInfos;
        }

        /// <summary>
        /// 相对这学期今天第几周。
        /// </summary>
        /// <returns></returns>
        public int GetWeekIndex(DateTime dateTime)
        {
            //Console.WriteLine((DateTime.Today - DateTime.Parse("2021-08-30")).Days / 7 + 1);
            return (dateTime - DateTime.Parse("2021-08-30")).Days / 7 + 1;
        }

        /// <summary>
        /// 获取当前是第几节课
        /// </summary>
        /// <returns></returns>
        public int GetIndexOfNow()
        {
            var list = TimeList;
            var nowSpan = DateTime.Now.TimeOfDay - TimeSpan.FromMinutes(30);
            var index = list.ToList().FindIndex(x => x.Item1 <= nowSpan && x.Item2 >= nowSpan);

            if (index == -1)
            {
                index = list.ToList().FindLastIndex(x => x.Item2 <= nowSpan);
            }

            return index + 1;
        }
    }
}