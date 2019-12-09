using System;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Rest.Lookups.V1;

namespace SecretSanta2._0
{
    public static class TwilioService
    {

        public static string LookupCarrier(string accountSid, string authToken, string phonenumber)
        {
            TwilioClient.Init(accountSid, authToken);

            var type = new List<string> {
                "carrier"
            };

            var phoneNumber = PhoneNumberResource.Fetch(
                type: type,
                pathPhoneNumber: new Twilio.Types.PhoneNumber(phonenumber)
            );

            //Console.WriteLine(phoneNumber.Carrier["name"]);
            //Console.WriteLine(phoneNumber.Carrier);

            return phoneNumber.Carrier["name"];
        }
    }
}
