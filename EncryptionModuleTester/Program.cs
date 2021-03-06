﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            var module = EncryptionModule.Connect("COM8");

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

            // Config examples
            Console.WriteLine("=== Config ===");
            
            // Stream chunk size
            module.SetStreamChunkSize(4);
            Console.WriteLine($"StreamChunkSize: {module.GetStreamChunkSize()}");

            //module.BeginStreamEncryption(data => { /**/ });
            /*while (true)
            {
                module.SendStreamData(Encoding.ASCII.GetBytes("test"));
               // Thread.Sleep(50);
            }*/
        }
    }
}