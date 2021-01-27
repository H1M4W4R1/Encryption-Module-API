namespace ITnnovative.EncryptionTool.API
{
    public class ConfigIdentifiers
    {
        
        public const byte CFG_STREAM_CHUNK_SIZE = 0x0;
        
        /// <summary>
        /// Fast USB mode - if enabled then device does not wait until data processing ends
        /// and immediately responds with USB_IDLE state.
        /// </summary>
        public const byte CFG_FAST_USB_MODE = 0x1;
    }
}