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
        public static List<string> QuarantineList = new();
        public static List<string> SIRList = new();
        public static Dictionary<string, int> TrendingList = new();
        public static List<string> Mentions = new();

        public static Message ProcessMessage(string header, string body)
        {
            Message message = new Message();
            List<string> tokenised = Helper.TokeniseOnLines(body);
            char messageTypeIdentifier = header[0];
            string id = header[1..];
            string sender = tokenised[0];
            tokenised.Remove(tokenised[0]);
            if (messageTypeIdentifier == 'S')
            {
                message = ProcessSMS(id, sender, tokenised);
            }
            else if (messageTypeIdentifier == 'E')
            {
                message = ProcessEmail(id, sender, tokenised);
            }
            else if (messageTypeIdentifier == 'T')
            {
                message = ProcessTweet(id, sender, tokenised);
            }
            else {
                throw new Exception("Invalid message type identifier");
            }

                JsonProcessor.SendToJSon(message);
            return message;
        }
        public static Tweet ProcessTweet(string id, string sender, List<string> body)
        {
            Tweet tweet1 = new()
            {
                ID = id,
                Sender = sender
            };
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
        public static SMS ProcessSMS(string id, string sender, List<string> body)
        {
            SMS sms1 = new()
            {
                ID = id,
                Sender = sender
            };
            string joined = string.Join(' ', body);
            List<string> tokenisedonspaces = Helper.TokeniseOnSpaces(joined);
            List<string> ExpandedTextSpeakMessage = ExpandTextSpeak(tokenisedonspaces);
            string joinedMessage = string.Join(" ", ExpandedTextSpeakMessage);
            sms1.MessageText = joinedMessage;

            return sms1;
        }

        public static Email ProcessEmail(string id, string sender, List<string> body)
        {
            // Check if email is a serious incident report and if so, process it in separate function
            if (body[0].Substring(0, 3) == "SIR")
            {
                if (Regex.IsMatch(body[0][4..], @"[0-3][0-9]/[0-1][0-9]/[0-9][0-9]"))
                {
                    return ProcessSIR(id, sender, body);
                }
            }
            return ProcessNormalEmail(id, sender, body);
        }

        public static Email ProcessNormalEmail(string id, string sender, List<string> body)
        {
            Email email1 = new()
            {
                ID = id,
                Sender = sender,
                Subject = body[0]
            };

            //Sender and subject no longer required
            body.RemoveRange(0, 1);

            string joined = string.Join(' ', body);
            List<string> tokenisedOnSpaces = Helper.TokeniseOnSpaces(joined);
            List<string> messageListWithURLsQuarantined = QuarantineURLs(tokenisedOnSpaces);
            string messageWithURLsQuarantined = string.Join(" ", messageListWithURLsQuarantined);
            email1.MessageText = messageWithURLsQuarantined;

            return email1;
        }
        public static SIREmail ProcessSIR(string id, string sender, List<string> body)
        {
            SIREmail sirEmail1 = new()
            {
                ID = id,
                Sender = sender,
                Subject = body[0],
                SortCode = body[1],
                IncidentNature = body[2]
            };

            SIRList.Add(sirEmail1.SortCode + " " + sirEmail1.IncidentNature);
            body.RemoveRange(0, 3);

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
