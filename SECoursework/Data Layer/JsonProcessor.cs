using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SECoursework.Business.Models;
using SECoursework.Business;
using Newtonsoft.Json;
using System.IO;

namespace SECoursework.Data_Layer
{
    internal class JsonProcessor
    {

        public static void SendToJSon(Message message)
        {
            List<Message> messages = new List<Message>();

            //Get list of messages from Json, append specified message to list and send back to Json.
            messages = DeSerializeJson();
            messages.Add(message);
            string json = JsonConvert.SerializeObject(messages, Formatting.Indented);
            File.WriteAllText("C:\\Users\\danie\\OneDrive - Edinburgh Napier University\\Year 3\\Software Engineering\\Coursework\\SECoursework\\SECoursework\\Data Layer\\message_list.json", json);
        }

        public static List<Message> DeSerializeJson()
        {
            string json = File.ReadAllText("C:\\Users\\danie\\OneDrive - Edinburgh Napier University\\Year 3\\Software Engineering\\Coursework\\SECoursework\\SECoursework\\Data Layer\\message_list.json");
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
