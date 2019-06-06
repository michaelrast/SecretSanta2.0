
public class Pairing
{
    public SSMember Santa { get; set; }
    public SSMember Receiver { get; set; }


    public Pairing(SSMember santa, SSMember receiver)
    {
        Santa = santa;
        Receiver = receiver;
    }
}