using ManagemateAPI.Management.M_Session.Input_Objects;

namespace ManagemateAPI.Encryption.Input_Objects
{
    public class Decrypt_Byte_Data
    {
        public Session_Data SessionData { get; set; }
        public byte[] EncryptedData { get; set; }
    }
}
