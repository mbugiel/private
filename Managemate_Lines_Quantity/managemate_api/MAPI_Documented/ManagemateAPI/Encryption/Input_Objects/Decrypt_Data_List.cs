using ManagemateAPI.Management.M_Session.Input_Objects;

namespace ManagemateAPI.Encryption.Input_Objects
{
    public class Decrypt_Data_List
    {
        public Session_Data SessionData { get; set; }
        public List<Encrypted_Object> DataToDecrypt { get; set; }
    }
}
