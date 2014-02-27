using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using MusicManager;
using System.IO;

namespace MusicServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Music Server is up and running!");
            try
            {
                StreamReader sr = new StreamReader ("config.ini");

                //MusicManager.MusicManager.CLAmpLocation = sr.ReadLine();
                MusicManager.MusicManager.RootDirectory = sr.ReadLine();

                ServiceHost selfHost = new ServiceHost(typeof(MusicWebService.MusicWebService));

                selfHost.Open();

                Console.WriteLine("The service is up and running on the following endpoints:");
                foreach (var se in selfHost.Description.Endpoints)
                {
                    Console.WriteLine((se as ServiceEndpoint).Address);
                }

                Console.ReadLine();
                selfHost.Close();
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Please make sure you have a config.ini file where your application executable lays. Read the README.txt file for more information.");
                Console.ReadLine();
            }
        }
    }
}
