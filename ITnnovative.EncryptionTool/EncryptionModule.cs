using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ITnnovative.EncryptionTool.API
{
    public class EncryptionModule
    {
        /// <summary>
        /// Device port
        /// </summary>
        private ReliableSerialPort _com;

        private int _baudRate = 115200;

        /// <summary>
        /// Sets Baud Rate
        /// </summary>
        /// <param name="rate"></param>
        public EncryptionModule SetBaudRate(int rate)
        {
            _baudRate = rate;
            return this;
        }
        
        /// <summary>
        /// Connects to device on COM port
        /// </summary>
        /// <param name="comName"></param>
        public EncryptionModule Connect(string comName)
        {
            Disconnect();
            _com = new ReliableSerialPort(comName, _baudRate, Parity.None, 8, StopBits.One);
            _com.ReadBufferSize = 128;
            _com.WriteBufferSize = 128;
            
            _com.Open();
            return this;
        }

        /// <summary>
        /// Try to disconnect the device (if is connected)
        /// </summary>
        public EncryptionModule Disconnect()
        {
            if (_com != null)
            {
                if(_com.IsOpen)
                    _com.Close();
            }

            return this;
        }

        /// <summary>
        /// Sets encryption module password
        /// </summary>
        /// <param name="pwdBytes">Up to 255-byte password phrase</param>
        /// <exception cref="ArgumentException">Password phrase too long (over 255 bytes)</exception>
        public EncryptionModule SetPassword(params byte[] pwdBytes)
        {
            if(pwdBytes.Length > 255)
                throw new ArgumentException("Max password length is 255 bytes.");

            var pwdArray = new byte[pwdBytes.Length + 2];
            pwdArray[0] = Commands.SET_PASSWORD;
            pwdArray[1] = (byte) pwdBytes.Length;
            for (var q = 0; q < pwdBytes.Length; q++)
                pwdArray[q + 2] = pwdBytes[q];

            _com.Write(pwdArray, 0, pwdArray.Length);
   
            return this;
        }
        
        /// <summary>
        ///  Sets encryption module password
        /// </summary>
        /// <param name="password">Up to 255-byte password phrase</param>
        /// <exception cref="ArgumentException">Password phrase too long (over 64 bytes)</exception>
        public EncryptionModule SetPassword(string password)
        {
            // Get bytes from password
            var pwdBytes = Encoding.ASCII.GetBytes(password);
            SetPassword(pwdBytes);
            return this;
        }

        /// <summary>
        /// Initializes cipher on device
        /// </summary>
        public EncryptionModule InitializeCipher()
        {
            // Send command
            _com.Write(new []{Commands.INIT_ENCRYPTION, Commands.DUMMY}, 0, 2);
            return this;
        }


        /// <summary>
        /// Encrypts sequence of data
        /// </summary>
        /// <param name="seq">Data sequence, max 255 characters</param>
        public byte[] EncryptSequence(params byte[] seq)
        {
            var arr = new byte[seq.Length];

            // Check seq. length
            if (seq.Length > 16384)
            {
                throw new ArgumentException($"Max sequence length is 16384 characters.");
            }

            // Get bytes for len (2-byte integer)
            var amount = BitConverter.GetBytes((ushort) seq.Length);
            
            // Write command
            _com.Write(new byte[]{Commands.ENCRYPT_SEQUENCE, amount[^1], amount[0]}, 0, 3);

            // Create listener
            var cOffset = 0;
            EventHandler<DataReceivedArgs> listener = (sender, args) =>
            {
                var len = args.Data.Length;
                for (var q = 0; q < len; q++)
                {
                    arr[q + cOffset] = args.Data[q];
                }
                cOffset += len;
            };
            _com.DataReceived += listener;
       
            var sw = new Stopwatch();
            sw.Start();
            
            // Send data
            _com.Write(seq, 0, seq.Length);
            
            // Wait for sequence to be received into buffer and read entire sequence at once to make it faster
            // It forces us to allocate around 1MB of RAM...
            while (cOffset < seq.Length)
            {
            }
            
            sw.Stop();
            Console.WriteLine("OK: " + sw.ElapsedMilliseconds + "ms");
            Console.WriteLine("Speed: " + 1000*((double)seq.Length / sw.ElapsedMilliseconds) + " B/s");

            // Remove listener
            _com.DataReceived -= listener;

            return arr;
        }

        /// <summary>
        /// Dump encryption data to use by VMPC on machine
        /// </summary>
        /// <returns></returns>
        public byte[] DumpEncryptionData()
        {
            var arr = new byte[258];
            
            // Create listener for downloading data
            var cOffset = 0;
            EventHandler<DataReceivedArgs> listener = (sender, args) =>
            {
                var len = args.Data.Length;
                for (var q = 0; q < len; q++)
                {
                    arr[q + cOffset] = args.Data[q];
                }
                cOffset += len;
            };
            _com.DataReceived += listener;

            // Send command to dump data
            _com.Write(new[] {Commands.DUMP_DATA, Commands.DUMMY}, 0, 2);
            
            // Wait until data is dumped
            while (cOffset < 258)
            {
            }

            // Remove listener
            _com.DataReceived -= listener;

            // Return dumped data
            return arr;
        }

        /// <summary>
        /// Load encryption data
        /// </summary>
        public void LoadEncryptionData(byte[] array)
        {
            // Check if size is correct
            if(array.Length != 258)
                throw new ArgumentException("Array length must equal 258 bytes.");
            
            // Build array incl. command
            var arrayNew = new byte[259];
            arrayNew[0] = Commands.LOAD_DATA;
            for (var q = 0; q < array.Length; q++)
            {
                arrayNew[q + 1] = array[q];
            }

            // Write new array
            _com.Write(arrayNew, 0, arrayNew.Length);
        }
        
        /// <summary>    
        /// Encrypts text sequence
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public byte[] EncryptSequence(string text)
        {
            return EncryptSequence(Encoding.ASCII.GetBytes(text));
        }

    }
}