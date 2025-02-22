using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace SECoursework.Data_Layer
{
    internal class TextSpeakProcessor
    {
        private static readonly string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Data Layer\textwords.csv");
        public static Dictionary<string, string> GetTextSpeak()
        {
            var dict = File.ReadLines(filePath).Select(line => line.Split(new[] {','}, 2)).ToDictionary(line => line[0], line => line[1]);
            return dict;
        }
    }
}
