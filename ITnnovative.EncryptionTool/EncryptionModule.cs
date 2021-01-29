using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ITnnovative.EncryptionTool.API.Tools;

namespace ITnnovative.EncryptionTool.API
{
    public class EncryptionModule
    {
        /// <summary>
        /// Device port
        /// </summary>
        private ReliableSerialPort _com;

        private int _baudRate = 9000000;
        private bool _streamingEncryption;
        private bool _connected;
        private const int HANDSHAKE_TIMEOUT = 5000; // ms

        /// <summary>
        /// Information about supported features, default: none (until downloaded)
        /// </summary>
        private byte[] _supportedFeatures = new byte[32];
        
        /// <summary>
        /// Block constructor for device
        /// </summary>
        private EncryptionModule(){}

        /// <summary>
        /// Check if command is supported
        /// </summary>
        public bool IsCommandSupported(byte commandCode)
        {
            // Calculate offsets for command bit
            var offset = commandCode / 8;
            var pos = commandCode % 8;

            // Check if command is supported
            return (_supportedFeatures[offset] & (128 >> pos)) > 0;
        }

        /// <summary>
        /// Gets list of supported commands
        /// </summary>
        public List<byte> GetSupportedCommands()
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            // Create list to return
            var list = new List<byte>();
            for (byte cmd = 0; cmd < 256; cmd++)
            {
                // Check if commands is supported and add it
                if (IsCommandSupported(cmd))
                    list.Add(cmd);
            }

            // Return list of supported commands
            return list;
        }
        
        /// <summary>
        /// Get stream chunk size
        /// </summary>
        public ushort GetStreamChunkSize()
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!"); 
            
            if(!IsCommandSupported(Commands.GET_CONFIG_VALUE))
                throw new NotSupportedException("This command is not supported on this device.");
            
            var arr = new byte[2];
            var cOffset = 0;
            
            // Download listener
            void Listener(object sender, DataReceivedArgs args)
            {
                var len = args.Data.Length;
                for (var q = 0; q < len; q++)
                {
                    arr[cOffset] = args.Data[q];
                    cOffset++;
                }
            }

            _com.DataReceived += Listener;

            // Send request
            _com.Write(
                new byte[] {Commands.GET_CONFIG_VALUE, ConfigIdentifiers.CFG_STREAM_CHUNK_SIZE, Commands.DUMMY}, 0,
                3);

            while (cOffset < 2)
            {
                // Do nothing, wait for data
            }
            
            _com.DataReceived -= Listener;

            // Convert back to ushort
            return BitConverter.ToUInt16(new byte[]{arr[^1], arr[0]});
        }
        
        /// <summary>
        /// Set stream chunk size to new value
        /// </summary>
        public EncryptionModule SetStreamChunkSize(ushort streamChunkSize)
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!"); 
            
            if(!IsCommandSupported(Commands.SET_CONFIG_VALUE))
                throw new NotSupportedException("This command is not supported on this device.");

            // Get value as byte array
            var bytes = BitConverter.GetBytes(streamChunkSize);
         
            // Send data to Encryption Module
            _com.Write(
                new byte[] {Commands.SET_CONFIG_VALUE, ConfigIdentifiers.CFG_STREAM_CHUNK_SIZE, bytes[^1], bytes[0]}, 0,
                4);
            
            return this;
        }
        
        /// <summary>
        /// Gets list of supported features
        /// </summary>
        public byte[] GetSupportedFeaturesBitData()
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            var arr = new byte[32];
            
            // Create listener
            var cOffset = 0;

            // Download listener
            void Listener(object sender, DataReceivedArgs args)
            {
                var len = args.Data.Length;
                for (var q = 0; q < len; q++)
                {
                    arr[cOffset] = args.Data[q];
                    cOffset++;
                }
            }

            _com.DataReceived += Listener;

            _com.Write(new byte[] {Commands.GET_FEATURES, Commands.DUMMY}, 0, 2);
            
            // Wait for handshake or device not recognized
            if (!SpinWait.SpinUntil(() => cOffset >= 32, HANDSHAKE_TIMEOUT))
            {
                
                throw new HardwareException("Device has not been recognized. Are you using correct port?");
            }
          

            _com.DataReceived -= Listener;
            return arr;
        }

        /// <summary>
        /// Connects to device on COM port
        /// </summary>
        public static EncryptionModule Connect(string comName)
        {
            var emo = new EncryptionModule();
            return emo.ConnectInternal(comName);
        }
        
        /// <summary>
        /// Connects to device on COM port
        /// </summary>
        private EncryptionModule ConnectInternal(string comName)
        {
            if(_connected)
                Disconnect();
            
            _com = new ReliableSerialPort(comName, _baudRate, Parity.None, 8, StopBits.One);
            _com.ReadBufferSize = 128;
            _com.WriteBufferSize = 128;

            try
            {
                _com.Open();
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new HardwareException("Device not found at provided port. Are you using proper value?");
            }

            // Connect to device
            _connected = true;

            // Get supported features for this device
            _supportedFeatures = GetSupportedFeaturesBitData();
            return this;
        }

        /// <summary>
        /// Try to disconnect the device (if is connected)
        /// </summary>
        public EncryptionModule Disconnect()
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (_com != null)
            {
                if(_com.IsOpen)
                    _com.Close();
            }

            _connected = false;

            return this;
        }

        /// <summary>
        /// Begins stream encryption.
        /// </summary>
        /// <param name="onStreamDataReceived">Event will be invoked each time data is received. Events are invoked in same order data is sent.</param>
        public EncryptionModule BeginStreamEncryption(Action<byte[]> onStreamDataReceived)
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (!IsCommandSupported(Commands.BEGIN_STREAM))
                throw new NotSupportedException("This command is not supported on this device.");
            
            _streamingEncryption = true;
            // Handler for data receiving
            void Listener(object sender, DataReceivedArgs args)
            {
                onStreamDataReceived(args.Data);
            }

            // Register handler
            _com.DataReceived += Listener;
            
            // Begin stream encryption
            _com.Write(new[] {Commands.BEGIN_STREAM, Commands.DUMMY}, 0, 2);
            return this;
        }

        /// <summary>
        /// Send Stream data to module. It will not return encrypted data. It will be returned to predefined action inside BeginStreamEncryption.
        /// </summary>
        public EncryptionModule SendStreamData(byte[] data)
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (!_streamingEncryption)
                throw new SystemException("You need to initialize stream encryption first. Call BeginStreamEncryption to do so.");
            _com.Write(data, 0, data.Length);
            return this;
        }
        
        /// <summary>
        /// Sets encryption module password
        /// </summary>
        /// <param name="pwdBytes">Up to 255-byte password phrase</param>
        /// <exception cref="ArgumentException">Password phrase too long (over 255 bytes)</exception>
        public EncryptionModule SetPassword(params byte[] pwdBytes)
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (!IsCommandSupported(Commands.SET_PASSWORD))
                throw new NotSupportedException("This command is not supported on this device.");
            
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
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
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
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (!IsCommandSupported(Commands.INIT_ENCRYPTION))
                throw new NotSupportedException("This command is not supported on this device.");
            
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
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (!IsCommandSupported(Commands.ENCRYPT_SEQUENCE))
                throw new NotSupportedException("This command is not supported on this device.");
            
            var arr = new byte[seq.Length];

            // Check seq. length
            if (seq.Length > 8192)
            {
                throw new ArgumentException($"Max sequence length is 8192 bytes.");
            }

            // Get bytes for len (2-byte integer)
            var amount = BitConverter.GetBytes((ushort) seq.Length);

            // Create listener
            var cOffset = 0;
            EventHandler<DataReceivedArgs> listener = (sender, args) =>
            {
              
                    var len = args.Data.Length;
                    for (var q = 0; q < len; q++)
                    {
                        arr[cOffset] = args.Data[q];
                        cOffset++;
                    }
              
            };
            _com.DataReceived += listener;

            // Build new list for array sending
            var narr = new List<byte>() {Commands.ENCRYPT_SEQUENCE, amount[^1], amount[0]};
            narr.AddRange(seq);
            
            // Write command
            _com.Write(narr.ToArray(), 0, narr.Count);

            
            // Send data
            //_com.Write(seq, 0, seq.Length);
            
            // Wait for sequence to be received into buffer and read entire sequence at once to make it faster
            // It forces us to allocate around 1MB of RAM...
            while (cOffset < seq.Length)
            {
            }
            
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
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (!IsCommandSupported(Commands.DUMP_DATA))
                throw new NotSupportedException("This command is not supported on this device.");
            
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
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (!IsCommandSupported(Commands.LOAD_DATA))
                throw new NotSupportedException("This command is not supported on this device.");
            
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
        /// Load encryption data
        /// </summary>
        public void LoadEncryptionData(byte[] p, byte s, byte n)
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            if (!IsCommandSupported(Commands.LOAD_DATA))
                throw new NotSupportedException("This command is not supported on this device.");
            
            // Check if size is correct
            if(p.Length != 256)
                throw new ArgumentException("P table length must be equal to 256 bytes!");
            
            // Build array incl. command
            var arrayNew = new byte[259];
            arrayNew[0] = Commands.LOAD_DATA;
            for (var q = 0; q < p.Length; q++)
            {
                arrayNew[q + 1] = p[q];
            }

            // Add s at last place - 1
            arrayNew[^2] = s;
            
            // Add n at last place
            arrayNew[^1] = n;

            // Write new array
            _com.Write(arrayNew, 0, arrayNew.Length);
        }

        /// <summary>    
        /// Encrypts text sequence
        /// </summary>
        public byte[] EncryptSequence(string text, Encoding encoding = null)
        {
            if (!_connected)
                throw new HardwareException("Encryption Module is not connected!");
            
            // If encoding is not set, then use ASCII
            if(encoding == null)
                encoding = Encoding.ASCII;
            
            return EncryptSequence(encoding.GetBytes(text));
        }

    }
}