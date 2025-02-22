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
        private string id = "";
        private string sender = "";
        private List<string> messageBody = new();

        public Message ProcessMessage(string header, string body)
        {
            Message message = new Message();
            this.messageBody = Helper.TokeniseOnLines(body);
            char messageTypeIdentifier = header[0];
            this.id = header[1..];
            this.sender = this.messageBody[0];
            this.messageBody.Remove(this.messageBody[0]);
            if (messageTypeIdentifier == 'S')
            {
                message = ProcessSMS();
            }
            else if (messageTypeIdentifier == 'E')
            {
                message = ProcessEmail();
            }
            else if (messageTypeIdentifier == 'T')
            {
                message = ProcessTweet();
            }
            else {
                throw new Exception("Invalid message type identifier");
            }
            JsonProcessor.SendToJSon(message);
            return message;
        }

        // Common method for processing message body
        private T CreateProcessedMessage<T>(Func<T> createMessageFunc, Action<T> addAdditionalDataFunc) where T : Message {
            T message = createMessageFunc(); // Create the message object

            string joined = string.Join(' ', this.messageBody);
            List<string> tokenisedOnSpaces = Helper.TokeniseOnSpaces(joined);
            List<string> expandedTextMessage = ExpandTextSpeak(tokenisedOnSpaces);
            string joinedMessage = string.Join(" ", expandedTextMessage);

            message.MessageText = joinedMessage; // Set common message text

            addAdditionalDataFunc(message); // Add any specific fields (like subject, sort code)

            return message;
        }

        public Tweet ProcessTweet() {
            return CreateProcessedMessage<Tweet>(
                () => new Tweet { ID = this.id, Sender = this.sender },  // Create Tweet object
                tweet => {
                    // Step 3: Specific logic for Tweet
                    GetHashtags(Helper.TokeniseOnSpaces(tweet.MessageText)); // Process hashtags
                    GetMentions(Helper.TokeniseOnSpaces(tweet.MessageText)); // Process mentions
                }
            );
        }

        public SMS ProcessSMS() {
            return CreateProcessedMessage<SMS>(
                () => new SMS { ID = this.id, Sender = this.sender },  // Create SMS
                sms => {
                    // SMS doesn't have additional logic, so no extra action is needed here
                }
            );
        }

        public Email ProcessEmail() {
            if (this.messageBody[0].Substring(0, 3) == "SIR") {
                if (Regex.IsMatch(this.messageBody[0][4..], @"[0-3][0-9]/[0-1][0-9]/[0-9][0-9]")) {
                    return ProcessSIR();
                }
            }
            return ProcessNormalEmail();
        }

        public Email ProcessNormalEmail() {
            return CreateProcessedMessage<Email>(
                () => new Email { ID = this.id, Sender = this.sender, Subject = this.messageBody[0] },  // Create Email
                email => {
                    email.Subject = this.messageBody[0];
                    this.messageBody.RemoveRange(0, 1); // Remove the subject
                    List<string> tokenisedOnSpaces = Helper.TokeniseOnSpaces(string.Join(" ", this.messageBody));
                    List<string> quarantinedMessage = QuarantineURLs(tokenisedOnSpaces);
                    email.MessageText = string.Join(" ", quarantinedMessage);
                }
            );
        }

        public SIREmail ProcessSIR() {
            return CreateProcessedMessage<SIREmail>(
                () => new SIREmail { ID = this.id, Sender = this.sender, Subject = this.messageBody[0] },  // Create SIREmail
                sirEmail => {
                    sirEmail.SortCode = this.messageBody[1];
                    sirEmail.IncidentNature = this.messageBody[2];
                    SIRList.Add(sirEmail.SortCode + " " + sirEmail.IncidentNature);
                    this.messageBody.RemoveRange(0, 3); // Remove the SIR-specific fields
                    List<string> tokenisedOnSpaces = Helper.TokeniseOnSpaces(string.Join(" ", this.messageBody));
                    List<string> quarantinedMessage = QuarantineURLs(tokenisedOnSpaces);
                    sirEmail.MessageText = string.Join(" ", quarantinedMessage);
                }
            );
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
