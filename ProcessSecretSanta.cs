using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace SecretSanta2._0
{
    public class ProcessSecretSanta
    {
        private List<SSMember> _members { get; set; }
        private IConfigurationSection _configurationSection {get;set;}
        private SendMail _sendMail {get;set;}
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
                _members = CompileMembersFromFile();

                Console.WriteLine($"Found {_members.Count} members in Secret Santa.");

                while (!PairingsValid(pairings))
                {
                    pairings = FindPairings();
                }

                Console.WriteLine($"Found Successful Pairings.");
                Console.WriteLine($"Sending Progress 0%.");
                var numberThrough = 0M;
                foreach (var pairing in pairings)
                {
                    //test area
                    var phoneNumber = pairing.Santa.PhoneNumber;
                    if (!production)
                        phoneNumber = testNumber;

                    _sendMail.SendToPhone(phoneNumber, $"{year} Secret Santa", $"You {pairing.Santa.Name} are {pairing.Receiver.Name}s Secret Santa! {this.extraMessage}");
                    numberThrough++;
                    decimal percentage = numberThrough / pairings.Count;
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
                var partnerOptions = _members.Where(i => i.Name != member.Name && i.Picked == false).ToList();

                if (partnerOptions.Count == 0)
                    continue;

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
        private List<SSMember> CompileMembersFromFile()
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