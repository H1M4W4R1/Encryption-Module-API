using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ITnnovative.EncryptionTool.API;

namespace EncryptionModuleTester
{
    class Program
    {
        static void Main(string[] args)
        {
   
            var module = new EncryptionModule();
            module.Connect("/dev/ttyACM0");
           
           

            var sequence = "test";
            /*for (var q = 0; q < (16384 - 4) / 4; q++)
            {
                sequence += "test";
            }*/

            //var data = module.EncryptSequence(sequence);
            while (true)
            {
                module.SetPassword("testtesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttest")
                    .InitializeCipher();
                module.EncryptSequence(sequence);
                Thread.Sleep(new TimeSpan(10 * 500));
            }
            //Console.WriteLine(data.GetDataString());
            
            //var data = module.EncryptSequence("th1515myb4nkp455w0rd");
            //Console.WriteLine(data.GetDataString());
            
            //module.SetPassword("randompassword")
             //   .InitializeCipher();
            //data = module.EncryptSequence(data);
            //Console.WriteLine(data.GetDataString());
            
            Console.ReadLine();
        }
    }
}