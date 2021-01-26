using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ITnnovative.EncryptionTool.API;
using ITnnovative.EncryptionTool.API.Tools;

namespace EncryptionModuleTester
{
    class Program
    {
        static void Main(string[] args)
        {


            // Connect to device at specific port
            var module = EncryptionModule.Connect("COM3");

            // Set password for VMPC
            module.SetPassword("test");

            // Initialize VMPC
            module.InitializeCipher();

            // Encrypt sequence
            var encrypted = module.EncryptSequence("test");

            Console.WriteLine(encrypted.GetDataString());

            // Reinitialize cipher
            module.InitializeCipher();

            // Decrypt sequence
            var decrypted = module.EncryptSequence(encrypted);

            Console.WriteLine(decrypted.GetDataString());

       
        }
    }
}