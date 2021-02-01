namespace ITnnovative.EncryptionTool.API.Tools
{
    public class EncryptionChecksum
    {
        public uint InputChecksum;
        public uint OutputChecksum;
        private EncryptionChecksum(){}

        /// <summary>
        /// Create new CRC Checksum
        /// </summary>
        public static EncryptionChecksum Create(uint input, uint output)
        {
            var crc = new EncryptionChecksum();
            crc.InputChecksum = input;
            crc.OutputChecksum = output;
            return crc;
        }
        
    }
}