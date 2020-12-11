using System;
using System.Collections.Generic;

public class SSMember
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public bool Picked { get; set; }
    public string Carrier { get; set; }
    public bool InAttendence {get;set;}
    public List<string> PreviousList {get;set; }
    public string Address { get; set; }

    public SSMember(string name, string phoneNumber, bool inAttendence, List<string> previousList, string address)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        Picked = false;
        InAttendence = inAttendence;
        PreviousList = previousList;
        Address = address;
    }

    public static SSMember FromCsv(string csvLine)
    {
        string[] values = csvLine.Split(',');
        SSMember member = new SSMember(values[0], values[1], bool.Parse(values[2]), new List<string>(values[3].Split('|')), values[4]);
        return member;
    }
}