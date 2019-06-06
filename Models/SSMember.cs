
public class SSMember
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public bool Picked { get; set; }

    public SSMember(string name, string phoneNumber)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        Picked = false;
    }

    public static SSMember FromCsv(string csvLine)
    {
        string[] values = csvLine.Split(',');
        SSMember member = new SSMember(values[0], values[1]);
        return member;
    }
}