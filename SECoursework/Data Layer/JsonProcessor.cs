using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SECoursework.Business.Models;
using SECoursework.Business;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Markup;

namespace SECoursework.Data_Layer
{
    internal class JsonProcessor
    {
        private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Data Layer\message_list.json");

        public static void SendToJSon(Message message)
        {
            List<Message> messages = new();
            messages = DeSerializeJson();
            messages.Add(message);
            string json = JsonConvert.SerializeObject(messages, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static List<Message> DeSerializeJson()
        {
            string json = File.ReadAllText(FilePath);
            List<Message> messages;
            if(JsonConvert.DeserializeObject<List<Message>>(json) == null)
            {
                messages = new List<Message>();
            }
            else
            {
                messages = JsonConvert.DeserializeObject<List<Message>>(json);
            }
            return messages;
        }
    }
}
