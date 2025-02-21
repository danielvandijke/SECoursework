using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SECoursework.Business.Models;
using SECoursework.Data_Layer;
using System.Windows;

namespace SECoursework.Business
{
    internal class MessageProcessor
    {
        public static List<string> QuarantineList = new List<string>();
        public static List<string> SIRList = new List<string>();
        public static Dictionary<string, int> TrendingList = new Dictionary<string, int>();
        public static List<string> Mentions = new List<string>();

        public static Message ProcessMessage(string header, string body)
        {
            Message message = new Message();
            List<string> tokenised = Helper.TokeniseOnLines(body);
            if (header[0] == 'S')
            {
                message = ProcessSMS(header, tokenised);
            }
            else if (header[0] == 'E')
            {
                message = ProcessEmail(header, tokenised);
            }
            else
            {
                message = ProcessTweet(header, tokenised);
            }

            JsonProcessor.SendToJSon(message);
            return message;
        }
        public static Tweet ProcessTweet(string header, List<string> body)
        {
            Tweet tweet1 = new()
            {
                ID = header[1..],
                Sender = body[0]
            };

            body.Remove(body[0]);

            
            string joined = string.Join(' ', body);
            List<string> tokenisedonspaces = Helper.TokeniseOnSpaces(joined);

            //Add hashtags to trending list
            GetHashtags(tokenisedonspaces);
            //Add mentions to mentions list
            GetMentions(tokenisedonspaces);
            //Expand text speak within tweet
            List<string> ExpandedTextSpeakMessage = ExpandTextSpeak(tokenisedonspaces);


            string joinedMessage = string.Join(" ", ExpandedTextSpeakMessage);
            tweet1.MessageText = joinedMessage;

 
            return tweet1;
        }
        public static SMS ProcessSMS(string header, List<string> body)
        {
            SMS sms1 = new()
            {
                ID = header[1..],
                Sender = body[0]
            };

            //Get message text with expanded text speak
            body.Remove(body[0]);
            string joined = string.Join(' ', body);
            List<string> tokenisedonspaces = Helper.TokeniseOnSpaces(joined);
            List<string> ExpandedTextSpeakMessage = ExpandTextSpeak(tokenisedonspaces);
            string joinedMessage = string.Join(" ", ExpandedTextSpeakMessage);
            sms1.MessageText = joinedMessage;

            return sms1;
        }

        public static Email ProcessEmail(string header, List<string> body)
        {
            // Check if email is a serious incident report and if so, process it in separate function
            if (body[1].Substring(0, 3) == "SIR")
            {
                if (Regex.IsMatch(body[1][4..], @"[0-3][0-9]/[0-1][0-9]/[0-9][0-9]"))
                {
                    return ProcessSIR(header, body);
                }
            }
            return ProcessNormalEmail(header, body);
        }

        public static Email ProcessNormalEmail(string header, List<string> body)
        {
            Email email1 = new()
            {
                ID = header[1..],
                Sender = body[0],
                Subject = body[1]
            };

            //Sender and subject no longer required
            body.RemoveRange(0, 2);

            string joined = string.Join(' ', body);
            List<string> tokenisedOnSpaces = Helper.TokeniseOnSpaces(joined);
            List<string> messageListWithURLsQuarantined = QuarantineURLs(tokenisedOnSpaces);
            string messageWithURLsQuarantined = string.Join(" ", messageListWithURLsQuarantined);
            email1.MessageText = messageWithURLsQuarantined;

            return email1;
        }
        public static SIREmail ProcessSIR(string header, List<string> body)
        {
            SIREmail sirEmail1 = new()
            {
                ID = header[1..],
                Sender = body[0],
                Subject = body[1],
                SortCode = body[2],
                IncidentNature = body[3]
            };

            //Add sort code and nature of incident to SIR list
            SIRList.Add(sirEmail1.SortCode + " " + sirEmail1.IncidentNature);

            //get rid of the above properties from body
            body.RemoveRange(0, 4);

            //Get message text with URLs quarantined
            string joined = string.Join(' ', body);
            List<string> tokenisedOnSpaces = Helper.TokeniseOnSpaces(joined);
            List<string> messageListWithURLsQuarantined = QuarantineURLs(tokenisedOnSpaces);
            string messageWithURLsQuarantined = string.Join(" ", messageListWithURLsQuarantined);
            sirEmail1.MessageText = messageWithURLsQuarantined;

            return sirEmail1;
        }

        public static List<string> ExpandTextSpeak(List<string> message)
        {
            Dictionary<string, string> textSpeak = TextSpeakProcessor.GetTextSpeak();
            for (int i = 0; i < message.Count ; i++)
            {
                if (textSpeak.ContainsKey(message[i]))
                {
                    message[i] = message[i] + " <" + textSpeak[message[i]] + ">";
                }
            }
            return message;
        }

        public static List<string> QuarantineURLs(List<string> message)
        {
            for (int i = 0; i < message.Count ; i++)
            {
                if (message[i].StartsWith("http://") || message[i].StartsWith("https://"))
                {
                    QuarantineList.Add(message[i]);
                    message[i] = "<URL Quarantined>";
                }
            }
            return message;
        }

        public static void GetHashtags(List<string> message)
        {
            // Adds hashtags to a dictionary that represents a trending list. If already added, increase number of times mentioned.
            for (int i = 0; i < message.Count; i++)
            {
                if(message[i].StartsWith("#"))
                {
                    if (TrendingList.ContainsKey(message[i]))
                    {
                        TrendingList[message[i]]++;
                    }
                    else
                    {
                        TrendingList.Add(message[i], 1);
                    }
                }
            }
        }

        public static void GetMentions(List<string> message)
        {
            for (int i = 0; i < message.Count; i++)
            {
                if (message[i].StartsWith("@"))
                {
                    Mentions.Add(message[i]);
                }
            }
        }

        public static List<string> GetSIRList()
        {
            return SIRList;
        }

        public static List<string> GetMentions()
        {
            return Mentions;
        }
        public static List<string> GetTrending()
        {
            var items = from pair in TrendingList
                        orderby pair.Value descending
                        select pair.Key;

            return items.ToList();
        }
        public static List<string> GetQuarantineList()
        {
            return QuarantineList;
        }
    }
}
