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

        public static void SendToJSon(Message message)
        {
            List<Message> messages = new List<Message>();
            messages = DeSerializeJson();
            messages.Add(message);
            string json = JsonConvert.SerializeObject(messages, Formatting.Indented);
            File.WriteAllText(@"C:\Users\danie\OneDrive\Desktop\SECoursework\SECoursework\Data Layer\message_list.json", json);
        }

        public static List<Message> DeSerializeJson()
        {
            string json = File.ReadAllText(@"C:\Users\danie\OneDrive\Desktop\SECoursework\SECoursework\Data Layer\message_list.json");
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
