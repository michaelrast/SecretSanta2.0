using System;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using SecretSanta2._0.Models;

namespace SecretSanta2._0
{
    public class SendMail
    {
        private IConfigurationSection _configurationSection {get;set;}
        private string emailAddress {get;set;}
        private string userName {get;set;}
        private string password {get;set;}

        int numberOfMessages;
        DateTime measureMessageTime = Convert.ToDateTime("1900-1-1");

        public SendMail(IConfigurationSection configurationSection)
        {
            _configurationSection = configurationSection;
            this.emailAddress = _configurationSection.GetValue<string>("email_address");
            this.userName = _configurationSection.GetValue<string>("email_address_userName");
            this.password = _configurationSection.GetValue<string>("email_address_password");
        }
        public void SendToPhone(string phoneNumber, string carrier, string subject, string body)
        {
            string toEmail = "";
            switch (carrier)
            {
                case (Carriers.Verizon):
                    toEmail = $"{phoneNumber}@vtext.com";
                    break;
                case (Carriers.ATT):
                    toEmail = $"{phoneNumber}@mms.att.net";
                    break;
                case (Carriers.TMobile):
                    toEmail = $"{phoneNumber}@tmomail.net";
                    break;
                case (Carriers.Sprint):
                    toEmail = $"{phoneNumber}@pm.sprint.com";
                    break;
                default:
                    Console.WriteLine("Could not resolve email for phone number: " + phoneNumber);
                    break;
            }
            //Send(subject, body, $"{phoneNumber}@vtext.com");//try sending it to verizon
            //Send(subject, body, $"{phoneNumber}@mms.att.net");//ATT
            //Send(subject, body, $"{phoneNumber}@pm.sprint.com");//sprint
            //Send(subject, body, $"{phoneNumber}@tmomail.net");//tmoble

            //Send(subject, body, $"{phoneNumber}@textmagic.com");//tmoble

            if (!string.IsNullOrEmpty(toEmail))
            {
                Send(subject, body, toEmail);
            }
        }

        public void Send(string subject, string body, string toAddress)
        {
            TimeSpan timespan = DateTime.Now - measureMessageTime;
            if (timespan.Hours > 1)
            {
                numberOfMessages = 0;
                measureMessageTime = DateTime.Now;
            }
            // failsafe the number of messages that can go out with a single hour
            if (numberOfMessages > 150)
            {
                return;
            }
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            MailAddress from = new MailAddress(this.emailAddress);
            char[] delimiterChars = { ';' };
            string[] addresses = toAddress.Split(delimiterChars);
            foreach (string address in addresses)
            {
                MailAddress to = new MailAddress(address);
                MailMessage message = new MailMessage(from, to);
                message.Subject = subject;
                message.Body = body;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                NetworkCredential credential = new NetworkCredential(this.userName, this.password);
                client.UseDefaultCredentials = false;
                client.Credentials = credential;
                client.EnableSsl = true;
                try
                {
                    client.Send(message);
                    numberOfMessages++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(message.Body);
                }
            }
        }
    }
}