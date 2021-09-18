using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Amiable.SDK.Tool.HIni;

namespace AmiLesson.Code
{
    public class Drawer
    {
        private List<LessonInfo> datas;

        public string MainColor { get; set; } = "62BAAC";

        public Drawer(List<LessonInfo> lessonInfos)
        {
            datas = lessonInfos;
        }

        public Bitmap Draw(string text = "未知的用户...", long qq = 0)
        {
            Console.WriteLine("[Ami课表助手]正在生成图像...");
            var bitmap1 = DrawHeader(text);
            var bitmap2 = DrawContent(qq);
            var bitmap = new Bitmap(800, bitmap1.Height + bitmap2.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(bitmap1, 0, 0);
                graphics.DrawImage(bitmap2, 0, bitmap1.Height);
                graphics.Save();
            }

            bitmap1.Dispose();
            bitmap2.Dispose();

            return bitmap;
        }

        public Bitmap DrawContent(long qq = 0)
        {
            var backFileInfo =
                new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Lessons", "backs", $"{qq}.png"));
            if (backFileInfo.Exists is false)
            {
                backFileInfo =
                    new FileInfo(Path.Combine(AppContext.BaseDirectory, "Ami", "Lessons", "backs", $"0.png"));
            }

            Bitmap bitmap = new Bitmap(800, datas.Count * 260);

            Graphics graphics = Graphics.FromImage(bitmap);
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];

                var rect = new Rectangle(0, 260 * i, 800, 260);

                using (var stream = File.Open(backFileInfo.FullName, FileMode.Open))
                {
                    using (var image = Image.FromStream(stream))
                    {
                        graphics.DrawImage(image, rect);
                    }
                }

                var brush = new SolidBrush(ColorTranslator.FromHtml("#" + MainColor));
                var nameFont = new Font("微软雅黑", 36, GraphicsUnit.Pixel);
                var font = new Font("微软雅黑", 22, GraphicsUnit.Pixel);

                Rectangle nameRect = new Rectangle(157, 49 + (i * 260), 400, 50);
                graphics.DrawString(data.Name, nameFont, brush, nameRect, new StringFormat()
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                });


                graphics.DrawString(
                    $"{Analyzer.TimeList[data.StartTime - 1].Item1:hh\\:mm}-{Analyzer.TimeList[data.EndTime - 1].Item2:hh\\:mm}",
                    font, brush, 146, 137 + (i * 260));
                graphics.DrawString($"第{data.StartTime}-{data.EndTime}节", font, Brushes.White, 680, 54 + (i * 260));
                graphics.DrawString($"{data.Where}", font, brush, 142, 197 + (i * 260));
                graphics.DrawString($"{data.Room}", font, brush, 371, 197 + (i * 260));
                graphics.DrawString($"{data.Teacher}", font, brush, 400, 137 + (i * 260));

                var tip = YouKnow.Get();
                var tipFont = new Font("微软雅黑", 16, GraphicsUnit.Pixel);

                var tipRect = new Rectangle(535, 168 + (i * 260), 219, 60);
                graphics.DrawString(tip, tipFont, brush, tipRect, new StringFormat()
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near
                });
            }

            graphics.Save();
            return bitmap;
        }

        public Bitmap DrawHeader(string text)
        {
            Bitmap bitmap = new Bitmap(800, 100);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.FillRectangle(Brushes.White, 0, 0, 800, 100);
                var brush = new SolidBrush(ColorTranslator.FromHtml("#" + MainColor));
                var nameFont = new Font("微软雅黑", 36, GraphicsUnit.Pixel);
                var rect = new Rectangle(0, 0, 500, 100);

                graphics.DrawString(text, nameFont, brush, rect,
                    new StringFormat() { LineAlignment = StringAlignment.Center });
                graphics.Save();
            }

            return bitmap;
        }
    }
}