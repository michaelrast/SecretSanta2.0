using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

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
        public void SendToPhone(string phoneNumber, string subject, string body)
        {
            //brute force method... this is a hack and needs to be fixed
            Send(subject, body, $"{phoneNumber}@vtext.com");//try sending it to verizon
            Send(subject, body, $"{phoneNumber}@mms.att.net");//ATT
            Send(subject, body, $"{phoneNumber}@pm.sprint.com");//sprint
            Send(subject, body, $"{phoneNumber}@tmomail.net");//tmoble
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