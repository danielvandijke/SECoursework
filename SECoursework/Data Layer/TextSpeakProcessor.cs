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
        public static Dictionary<string, string> GetTextSpeak()
        {
            var dict = File.ReadLines("C:\\Users\\danie\\OneDrive\\Desktop\\SECoursework\\SECoursework\\Data Layer\\textwords.csv").Select(line => line.Split(new[] {','}, 2)).ToDictionary(line => line[0], line => line[1]);
            return dict;
        }
    }
}
