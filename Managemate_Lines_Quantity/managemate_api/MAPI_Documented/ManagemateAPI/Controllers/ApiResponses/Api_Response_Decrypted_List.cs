using ManagemateAPI.Encryption.Input_Objects;

namespace ManagemateAPI.Controllers
{
    public class Api_Response_Decrypted_List
    {
        public string code { get; set; }
        public string message { get; set; }
        public List<Decrypted_Object> responseData { get; set; }
    }

}
