using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SecretSanta2._0
{
    class Program
    {
        static IConfigurationSection _configurationSection;
        List<SSMember> ssMembers;

        static void Main(string[] args)
        {
            ConfigureServices();

            ProcessSecretSanta pss = new ProcessSecretSanta(_configurationSection);

            Console.WriteLine("Hit p for production or t for test.");

            var processed = false;

            while (!processed)
            {
                var key = Console.ReadKey();

                switch (key.KeyChar)
                {
                    case 'p':
                        pss.Process(true);
                        processed = true;
                        break;
                    case 't':
                        pss.Process(false);
                        processed = true;
                        break;
                    default:
                        Console.WriteLine("Could not recognize key.  Please hit p for production or t for test.");
                        break;
                }
            }
        }
 
        static private void ConfigureServices()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();
            
            _configurationSection = configuration.GetSection("Configuration");
        }
    }
}
