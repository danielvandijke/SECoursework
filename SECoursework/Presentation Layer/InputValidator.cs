using SECoursework.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SECoursework.Presentation_Layer
{
    internal class InputValidator
    {
        private static List<string> incidents = new List<string>
        {
            "Theft",
            "Staff Attack",
            "ATM Theft",
            "Raid",
            "Customer Attack",
            "Staff Abuse",
            "Bomb Threat",
            "Terrorism",
            "Suspicious Incident",
            "Intelligence",
            "Cash Loss"
        };

        public static bool ValidateMessage(string header, string body)
        {
            if (!ValidateHeader(header))
            {
                return false;
            }
            if (!ValidateBody(body, header[0]))
            {
                return false;
            }

            return true;
        }

        public static bool ValidateBody(string body, char c)
        {
            List<string> tokenised = Helper.TokeniseOnLines(body);
            // Identify the message type and validate the body accordingly
            bool validated = false;
            if (c == 'S')
            {
                validated = ValidateSMS(tokenised);
            }
            else if (c == 'E')
            {
                validated = ValidateEmail(tokenised);
            }
            else
            {
                validated = ValidateTweet(tokenised);
            }
            return validated;
        }
        public static bool ValidateHeader(string header)
        {

            if (header[0] != 'S' && header[0] != 'E' && header[0] != 'T')
            {
                MessageBox.Show("Header should start with an identifier: 'S', 'E' or 'T'. You typed " + header[0]);
                return false;
            }
            if (header.Length != 10)
            {
                MessageBox.Show("Header should be an identifier followed by a 9-digit ID code");
                return false;
            }
            foreach (char c in header.Substring(1))
            {
                bool isInt = Char.IsDigit(c);
                if (isInt == false)
                {
                    MessageBox.Show("ID code should consist only of integers");
                    return false;
                }
            }
            return true;
        }

        public static bool ValidateEmail(List<string> body)
        {
            if (!body[0].Contains('@'))
            {
                MessageBox.Show("Invalid email address. Must contain '@'");
                return false;
            }
            if(body.Count < 3)
            {
                MessageBox.Show("Emails must contain an email address, subject, and message, all on separate lines");
                return false;
            }
            if(body[1].Length == 0)
            {
                MessageBox.Show("Email must contain a subject on line 2");
                return false;
            }
            if (body[1].Length > 20)
            {
                MessageBox.Show("Subject line cannot be longer than 20 characters");
                return false;
            }
            if(GetMessageLength(body, 2) == 0)
            {
                MessageBox.Show("Email must contain a message of at least one character");
                return false;
            }
            if(GetMessageLength(body, 2) > 1028)
            {
                MessageBox.Show("Message text cannot be longer than 1028 characters");
                return false;
            }
            if (body[1][..3] == "SIR")
            {
                if(Regex.IsMatch(body[1][4..], @"[0-3][0-9]/[0-1][0-9]/[0-9][0-9]"))
                {
                    return ValidateSIR(body);
                }
            }
            return true;
        }

        public static bool ValidateSMS(List<string> body)
        {
            if(body[0].Length > 20)
            {
                MessageBox.Show("Phone number too long. Please enter a valid phone number on the first line");
                return false;
            }
            foreach (char c in body[0])
            {
                bool isInt = Char.IsDigit(c);
                if (isInt == false)
                {
                    MessageBox.Show("Phone number should consist only of integers.");
                    return false;
                }
            }
            if(body.Count < 2)
            {
                MessageBox.Show("SMS should contain at least a phone number and a message");
                return false;
            }
            if(GetMessageLength(body, 1) > 140)
            {
                MessageBox.Show("Message cannot be longer than 140 characters");
                return false;
            }
            return true;
        }

        public static bool ValidateSIR(List<string> body)
        {
            if (!Regex.IsMatch(body[2], @"[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
            {
                MessageBox.Show("First line of the message in a Serious Incident Report should contain a valid sort code only");
                return false;
            }
            if(body.Count < 5)
            {
                MessageBox.Show("Serious incident reports must contain an address, subject, sort code, nature and message all on separate lines.");
                return false;
            }
            if(!incidents.Contains(body[3]))
            {
                MessageBox.Show("Second line of the message in a Serious Incident Report must contain an incident of a valid nature");
                return false;
            }
            return true;
        }

        public static bool ValidateTweet(List<string> body)
        {
            if(body[0].Substring(0, 1) != "@")
            {
                MessageBox.Show("Twitter handle should start with an '@'.");
                return false;
            }
            if(body[0].Length < 2)
            {
                MessageBox.Show("Invalid Twitter handle. Should be of type '@example'.");
                return false;
            }
            foreach(char c in body[0].Substring(1))
            {
                bool isLetterOrNum = Char.IsLetterOrDigit(c);
                if(isLetterOrNum == false && c != '_')
                {
                    MessageBox.Show("Twitter handle can only contain letters, numbers or underscores.");
                    return false;
                }
            }
            if(body[0].Length > 16)
            {
                MessageBox.Show("Twitter handle should be no more than 15 characters");
                return false;
            }
            if(body.Count < 2)
            {
                MessageBox.Show("Tweet should contain a message");
                return false;
            }
            if(GetMessageLength(body, 1) > 140)
            {
                MessageBox.Show("Message text cannot be longer than 140 characters");
                return false;
            }
            return true;
        }

        public static int GetMessageLength(List<string> message, int start_line)
        {
            int message_length = 0;
            for (int i = start_line; i < message.Count; i++)
            {
                message_length += message[i].Length;
            }
            return message_length;
        }
    }
}
