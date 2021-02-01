namespace ITnnovative.EncryptionTool.API
{
    public static class Commands
    {
        
        /// <summary>
        /// Dummy command
        /// </summary>
        public const byte DUMMY = 0x0;
        
        /// <summary>
        /// Set password in-device (private key)
        /// </summary>
        public const byte SET_PASSWORD = 0x4;

        /// <summary>
        /// Initialize cipher on device (or reinitialize it)
        /// </summary>
        public const byte INIT_ENCRYPTION = 0x5;

        /// <summary>
        /// Begin stream command
        /// </summary>
        public const byte BEGIN_STREAM = 0x6;
        
        /// <summary>
        /// Encrypts sequence of data
        /// </summary>
        public const byte ENCRYPT_SEQUENCE = 0x7;
        
        /// <summary>
        /// Get Checksum from encryption
        /// </summary>
        public const byte GET_CHECKSUM = 0x10;
        
        /// <summary>
        /// Dumps serialization data
        /// </summary>
        public const byte DUMP_DATA = 0x50;
        
        /// <summary>
        /// Loads serialization data
        /// </summary>
        public const byte LOAD_DATA = 0x51;
        
        /// <summary>
        /// Get features of device as list
        /// </summary>
        public const byte GET_FEATURES = 0xF0;
        
        /// <summary>
        /// Command set cfg value
        /// </summary>
        public const byte SET_CONFIG_VALUE = 0xF2;

        /// <summary>
        /// Command get config value
        /// </summary>
        public const byte GET_CONFIG_VALUE = 0XF1;

    }
}