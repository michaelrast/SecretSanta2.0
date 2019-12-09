using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SecretSanta2._0.Models;

namespace SecretSanta2._0
{
    public class ProcessSecretSanta
    {
        private List<SSMember> _members { get; set; }
        private IConfigurationSection _configurationSection {get;set;}
        private SendMail _sendMail {get;set; }
        private string testNumber {get;set;}
        private string year {get;set;}
        private string extraMessage {get;set;}

        public ProcessSecretSanta(IConfigurationSection configurationSection)
        {
            _configurationSection = configurationSection;
            _sendMail = new SendMail(_configurationSection);

            this.testNumber = _configurationSection.GetValue<string>("test_number");
            this.year = _configurationSection.GetValue<string>("year");
            this.extraMessage = _configurationSection.GetValue<string>("extra_message");
        }

        //*
        //  Process Secret Santa
        // */
        public void Process(bool production)
        {
            try
            {
                List<Pairing> pairings = new List<Pairing>();
                _members = CompileMembersFromFile(production);

                Console.WriteLine($"Found {_members.Count} members in Secret Santa.");

                if (_members.Any(i =>
                    string.IsNullOrEmpty(i.Carrier) || (i.Carrier != Carriers.Verizon && i.Carrier != Carriers.ATT &&
                    i.Carrier != Carriers.TMobile && i.Carrier != Carriers.Sprint)))
                {
                    Console.WriteLine("Could not resolve all user's Carriers.");
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    return;
                }

                var attempts = 0;
                while (!PairingsValid(pairings))
                {
                    attempts++;
                    if(attempts > 1000){
                        throw new Exception("Could not find addequate pairings in specified time. Try rerunning.");
                    }
                    pairings = FindPairings();
                }

                Console.WriteLine($"Found Successful Pairings ({attempts} pairing tries).");
                foreach(var pairing in pairings){
                    Console.WriteLine($"You {pairing.Santa.Name} are {pairing.Receiver.Name}'s Secret Santa! {this.extraMessage}");
                }


                Console.WriteLine($"Hit y to continue or n to stop.");

                var key = Console.ReadKey();

                switch (key.KeyChar)
                {
                    case 'y':
                        break;
                    case 'n':
                        Console.WriteLine($"Stopping");
                        return;
                    default:
                        Console.WriteLine($"Follow directions!");
                        return;
                }

                Console.WriteLine($"Sending Progress 0%.");
                var numberThrough = 0M;
                foreach (var pairing in pairings)
                {
                    _sendMail.SendToPhone(pairing.Santa.PhoneNumber, pairing.Santa.Carrier, $"{year} Secret Santa", $"You {pairing.Santa.Name} are {pairing.Receiver.Name}'s Secret Santa! {this.extraMessage}");
                    //_sendText.Send();
                    numberThrough++;
                    decimal percentage = (numberThrough / pairings.Count)*100;
                    Console.WriteLine($"Sending Progress {percentage}%.");
                }

                Console.WriteLine($"Successfully sent all messages.");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception!");
                Console.WriteLine(ex.Message);
            }
            
            Console.ReadKey();
        }
        
        //*
        //  Will randomly assign people their pairings
        // */
        private List<Pairing> FindPairings()
        {
            List<Pairing> returnList = new List<Pairing>();

            Random rnd = new Random();

            foreach (var member in _members)
            {
                var partnerOptions = _members.Where(i => i.Name != member.Name && i.Picked == false && i.InAttendence == member.InAttendence).ToList();

                if (partnerOptions.Count == 0)
                    continue;

                partnerOptions = partnerOptions.OrderBy(a => Guid.NewGuid()).ToList();

                var randomNumber = rnd.Next(0, partnerOptions.Count - 1);
                var partner = partnerOptions[randomNumber];
                partner.Picked = true;
                returnList.Add(new Pairing(member, partner));
            }

            return returnList;
        }

        //*
        //  Will take in a list of Pairings and make sure they are all valid
        //  meaning no one has themselves
        // */
        private bool PairingsValid(List<Pairing> pairings)
        {
            if (pairings.Count != _members.Count)
            {
                ResetPicked();
                return false;
            }

            foreach (var pairing in pairings)
            {
                if (pairing.Santa.Name == pairing.Receiver.Name)
                {
                    ResetPicked();
                    return false;
                }
            }

            return true;
        }

        //*
        //  Will reset everyones picked status and mark them as not picked
        // */
        private void ResetPicked()
        {
            foreach (var skoMember in _members)
                skoMember.Picked = false;
        }

        //*
        //  will retrieve the file from the path set in the config file and will read the csv
        // */
        private List<SSMember> CompileMembersFromFile(bool production)
        {
            try
            {
                var filePath = _configurationSection.GetValue<string>("contact_file");
                Console.WriteLine($"File Path: {filePath}");

                var extension = Path.GetExtension(filePath);
                if(extension != ".csv")
                {
                    throw new Exception("ERROR: Input file must be a csv file.");
                }

                List<SSMember> returnList = File.ReadAllLines(filePath)
                                            .Select(v => SSMember.FromCsv(v))
                                            .ToList();

                foreach (var ssMember in returnList)
                {
                    //test area
                    if (!production)
                        ssMember.PhoneNumber = testNumber;

                    string accountSid = _configurationSection.GetValue<string>("twilio_sid");
                    string authToken = _configurationSection.GetValue<string>("twilio_authToken");
                    ssMember.Carrier = TwilioService.LookupCarrier(accountSid, authToken, ssMember.PhoneNumber);

                    Console.WriteLine(ssMember.Carrier);
                }

                return returnList;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error reading file!!!");
                Console.WriteLine("The input file must be a csv file with format {name},{phonenumber}");

                throw ex;
            }
        }
    }
}