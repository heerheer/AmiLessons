using System.Text.Json.Serialization;

namespace AmiLesson.Code
{
    public class LessonInfo
    {
        public string Name { get; set; }
        public int StartWeek { get; set; }
        public int EndWeek { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public string Teacher { get; set; }
        public string Where { get; set; }
        public string Room { get; set; }
        public int Week { get; set; }

        public int Mode { get; set; } = 0; //单:1 双:2
    }
}