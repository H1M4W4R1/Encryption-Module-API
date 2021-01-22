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
            module.Connect("/dev/ttyACM1");


            
            Console.WriteLine(module.IsCommandSupported(Commands.ENCRYPT_SEQUENCE));
        }
    }
}