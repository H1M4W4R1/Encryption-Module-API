using System;

namespace ITnnovative.EncryptionTool.API.Tools
{
    public class HardwareException : Exception
    {

        public HardwareException(string msg) : base(msg)
        { }
        
        public HardwareException(string msg, Exception inner) : base(msg, inner)
        { }
    }
}