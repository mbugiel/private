
namespace ManagemateAPI.Controllers
{
    public class Api_Response_Encrypted
    {
        public string code { get; set; }
        public string message { get; set; }
        public byte[] responseData { get; set; }
    }

}
