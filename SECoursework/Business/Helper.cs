using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECoursework.Business
{
    public class Helper
    {
        public static List<string> TokeniseOnLines(string text)
        {
            string[] tokenised = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            List<string> tokenised_array = new(tokenised);
            return tokenised_array;
        }

        public static List<string> TokeniseOnSpaces(string text)
        {
            string[] tokenised = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            List<string> tokenised_array = new(tokenised);
            return tokenised_array;
        }
    }
}
