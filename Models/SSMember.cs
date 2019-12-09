
public class SSMember
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public bool Picked { get; set; }
    public string Carrier { get; set; }
    public bool InAttendence {get;set;}

    public SSMember(string name, string phoneNumber, bool inAttendence)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        Picked = false;
        InAttendence = inAttendence;
    }

    public static SSMember FromCsv(string csvLine)
    {
        string[] values = csvLine.Split(',');
        SSMember member = new SSMember(values[0], values[1], bool.Parse(values[2]));
        return member;
    }
}