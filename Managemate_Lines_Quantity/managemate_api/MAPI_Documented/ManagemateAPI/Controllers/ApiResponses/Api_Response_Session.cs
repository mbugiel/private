using ManagemateAPI.Management.M_Session.Input_Objects;

namespace ManagemateAPI.Controllers
{
    public class Api_Response_Session
    {
        public string code { get; set; }
        public string message { get; set; }
        public Session_Data responseData { get; set; }
    }

}
