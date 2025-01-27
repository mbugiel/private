using ManagemateAPI.Management.M_Session.Input_Objects;

namespace ManagemateAPI.Management.M_User.AuthAPI_Input_Objects
{
    public class AuthAPI_Update_Passwd_Data
    {
        public Session_Data SessionData { get; set; }
        public string Password { get; set; }
        public string newPassword { get; set; }

    }
}
